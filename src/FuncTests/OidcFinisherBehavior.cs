using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using MyLab.ApiClient.Test;
using MyLab.OidcFinisher;
using MyLab.OidcFinisher.ApiSpecs.BizLogicApi;
using MyLab.OidcFinisher.ApiSpecs.OidcProvider;
using Xunit.Abstractions;

namespace FuncTests
{
    public class OidcFinisherBehavior : IClassFixture<TestApiFixture<Program, IOidcFinisherApiV1>>
    {
        private readonly TestApiFixture<Program, IOidcFinisherApiV1> _fxt;

        const string TestJohnDoeIdToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        
        readonly string _testAccessToken;
        readonly string _testRefreshToken;
        readonly string _testAuthCode;
        readonly string _testAuthState;

        private readonly Action<OidcFinisherOptions> _optionsHandler = opt =>
        {
            opt.ClientId = "foo";
            opt.ClientSecret = "bar";
            opt.RedirectUri = "baz";
            opt.AutoAccept = false;
        };

        private readonly TokenResponseDto _testTokenResponse;

        public OidcFinisherBehavior(TestApiFixture<Program, IOidcFinisherApiV1> fxt, ITestOutputHelper output)
        {
            _fxt = fxt;
            fxt.Output = output;

            _testAccessToken = Guid.NewGuid().ToString("N");
            _testRefreshToken = Guid.NewGuid().ToString("N");
            _testAuthCode = Guid.NewGuid().ToString("N");
            _testAuthState = Guid.NewGuid().ToString("N");

            _testTokenResponse = new TokenResponseDto
            {
                IdToken = TestJohnDoeIdToken,
                AccessToken = _testAccessToken,
                RefreshToken = _testRefreshToken,
                ExpiresIn = 100,
                TokenType = "code"
            };
        }

        [Theory]
        [InlineData("foo", "bar", "baz", true)]
        [InlineData(null, "bar", "baz", false)]
        [InlineData("foo", null, "baz", false)]
        [InlineData("foo", "bar", null, false)]
        public void ShouldValidateOptions(string clientId, string clientSecret, string redirectUri, bool validCombination)
        {
            //Arrange
            Action<OidcFinisherOptions> optionsHandler = opt =>
            {
                opt.ClientId = clientId;
                opt.ClientSecret = clientSecret;
                opt.RedirectUri = redirectUri;
            };

            OptionsValidationException? validationException = null;

            //Act
            try
            {
                _fxt.StartWithProxy(srv => srv.Configure(optionsHandler));
            }
            catch (OptionsValidationException e)
            {
                validationException = e;
            }

            //Assert
            Assert.Equal(validCombination, validationException == null);
        }

        [Fact]
        public async Task ShouldAutoAccept()
        {
            //Arrange
            var oidcProviderMock = new Mock<IOidcProvider>();
            oidcProviderMock.Setup(p => p.GetTokenAsync
                (
                    It.IsAny<TokenRequestDto>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(_testTokenResponse);

            var proxy = _fxt.StartWithProxy
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.Configure<OidcFinisherOptions>(opt => opt.AutoAccept = true);
                    srv.AddSingleton(oidcProviderMock.Object);
                }).ApiClient;
            
            //Act
            var finishResult = await proxy.FinishAsync(_testAuthCode, _testAuthState);

            //Assert
            Assert.NotNull(finishResult);
            Assert.Equal(_testAccessToken, finishResult.AccessToken);
            Assert.Equal(_testRefreshToken, finishResult.RefreshToken);
            Assert.Equal(TestJohnDoeIdToken, finishResult.IdToken);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        public async Task ShouldAccept(bool provideAccessToken, bool provideRefreshToken, bool provideIdToken)
        {
            //Arrange
            var oidcProviderMock = new Mock<IOidcProvider>();
            oidcProviderMock.Setup(p => p.GetTokenAsync
                (
                    It.IsAny<TokenRequestDto>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(_testTokenResponse);

            var bizLogicApiMock = new Mock<IBizLogicApi>();
            bizLogicApiMock.Setup(api => api.AcceptAsync(It.IsAny<ClientAcceptRequestDto>()))
                .ReturnsAsync
                (
                    new Func<ClientAcceptRequestDto, ClientAcceptResponseDto>
                    (
                        _ => new ClientAcceptResponseDto
                        {
                            Accept = true,
                            AddHeaders = new Dictionary<string, string>
                            {
                                {"X-Foo", "bar"}
                            },
                            ProvideAccessToken = provideAccessToken,
                            ProvideIdToken = provideIdToken,
                            ProvideRefreshToken = provideRefreshToken
                        })
                );

            var client = _fxt.Start
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.AddSingleton(oidcProviderMock.Object);
                    srv.AddSingleton(bizLogicApiMock.Object);
                }).ApiClient;

            //Act
            var finishResultCallDetails = await client.Call(api => api.FinishAsync(_testAuthCode, _testAuthState));

            //Assert
            Assert.NotNull(finishResultCallDetails.ResponseContent);
            Assert.True((provideAccessToken ? _testAccessToken : null) == finishResultCallDetails.ResponseContent.AccessToken);
            Assert.True((provideRefreshToken ? _testRefreshToken : null) == finishResultCallDetails.ResponseContent.RefreshToken);
            Assert.True((provideIdToken ? TestJohnDoeIdToken : null) == finishResultCallDetails.ResponseContent.IdToken);

            Assert.Contains
            (
                finishResultCallDetails.ResponseMessage.Headers,
                h => 
                    h.Key == "X-Foo" &&
                    h.Value.Any(v => v == "bar") &&
                    h.Value.Count() == 1
            );
        }

        [Fact]
        public async Task ShouldReject()
        {
            //Arrange
            var oidcProviderMock = new Mock<IOidcProvider>();
            oidcProviderMock.Setup(p => p.GetTokenAsync
                    (
                        It.IsAny<TokenRequestDto>(),
                        It.IsAny<string>()
                    ))
                .ReturnsAsync(_testTokenResponse);

            var bizLogicApiMock = new Mock<IBizLogicApi>();
            bizLogicApiMock.Setup(api => api.AcceptAsync(It.IsAny<ClientAcceptRequestDto>()))
                .ReturnsAsync
                (
                    new Func<ClientAcceptRequestDto, ClientAcceptResponseDto>
                    (
                        _ => new ClientAcceptResponseDto
                        {
                            Accept = false,
                            RejectionReason = "test reason"
                        })
                );

            var client = _fxt.Start
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.AddSingleton(oidcProviderMock.Object);
                    srv.AddSingleton(bizLogicApiMock.Object);
                }).ApiClient;

            //Act
            var finishResultCallDetails = await client.Call(api => api.FinishAsync(_testAuthCode, _testAuthState));

            var contentStream = await finishResultCallDetails.ResponseMessage.Content.ReadAsStreamAsync();
            contentStream.Seek(0, SeekOrigin.Begin);
            var contentReader = new StreamReader(contentStream);
            var stringContent = await contentReader.ReadToEndAsync();

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, finishResultCallDetails.StatusCode);
            Assert.Equal("test reason", stringContent);
        }



        [Fact]
        public async Task ShouldSendTokenResultToLogicApi()
        {
            //Arrange
            var oidcProviderMock = new Mock<IOidcProvider>();
            oidcProviderMock.Setup(p => p.GetTokenAsync
                (
                    It.IsAny<TokenRequestDto>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(_testTokenResponse);

            ClientAcceptRequestDto? acceptRequest = null;

            var bizLogicApiMock = new Mock<IBizLogicApi>();
            bizLogicApiMock.Setup(api => api.AcceptAsync(It.IsAny<ClientAcceptRequestDto>()))
                .ReturnsAsync
                    (
                        new Func<ClientAcceptRequestDto, ClientAcceptResponseDto>
                            (
                                req =>
                                {
                                    acceptRequest = req;
                                    return new ClientAcceptResponseDto
                                    {
                                        Accept = true
                                    };
                                })
                    );

            var proxy = _fxt.StartWithProxy
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.AddSingleton(oidcProviderMock.Object);
                    srv.AddSingleton(bizLogicApiMock.Object);
                }).ApiClient;
            
            //Act
            await proxy.FinishAsync(_testAuthCode, _testAuthState);

            //Assert
            Assert.NotNull(acceptRequest);
            Assert.Equal(_testAccessToken, acceptRequest.TokenResponse.AccessToken);
            Assert.Equal(_testRefreshToken, acceptRequest.TokenResponse.RefreshToken);
            Assert.Equal(TestJohnDoeIdToken, acceptRequest.TokenResponse.IdToken);
            Assert.Equal("code",acceptRequest.TokenResponse.TokenType);
            Assert.Equal(100u,acceptRequest.TokenResponse.ExpiresIn);
            Assert.NotNull(acceptRequest.IdToken);
            Assert.Contains(acceptRequest.IdToken, c => c is { Type:"name", Value:"John Doe" });
        }
    }
}