using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using MyLab.ApiClient;
using MyLab.ApiClient.Test;
using MyLab.Log.XUnit;
using MyLab.OidcFinisher;
using MyLab.OidcFinisher.ApiSpecs.BizLogicApi;
using MyLab.OidcFinisher.Features.Finish;
using Xunit.Abstractions;

namespace FuncTests
{
    public partial class OidcFinisherBehavior 
    {
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
                _fxt.StartWithProxy
                (srv => srv
                    .Configure(optionsHandler)
                    .AddLogging(c => c.AddXUnit(_output))
                );
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
            var oidcProviderMock = CreateOidcProviderMock();

            var proxy = _fxt.StartWithProxy
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.Configure<OidcFinisherOptions>(opt => opt.AutoAccept = true);
                    srv.AddSingleton(oidcProviderMock);
                    AddXUnit(srv);
                }).ApiClient;
            
            //Act
            var finishResult = await proxy.FinishAsync(_testAuthCode, _testAuthState, _testCodeVerifier);

            //Assert
            Assert.NotNull(finishResult);
            Assert.Equal(_testAccessToken, finishResult.AccessToken);
            Assert.Equal(_testRefreshToken, finishResult.RefreshToken);
            Assert.Equal(TestJohnDoeIdToken, finishResult.IdToken);
            Assert.Equal(_testExpiresIn, finishResult.ExpiresIn);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        public async Task ShouldAccept(bool provideAccessToken, bool provideRefreshToken, bool provideIdToken)
        {
            //Arrange
            var oidcProviderMock = CreateOidcProviderMock();

            var bizLogicApiMock = new Mock<IBizLogicApi>();
            bizLogicApiMock.Setup(api => api.AcceptDetailedAsync(It.IsAny<ClientAcceptRequestDto>()))
                .ReturnsAsync
                (
                    new Func<ClientAcceptRequestDto, CallDetails<ClientAcceptResponseDto>>
                    (
                        _ => new CallDetails<ClientAcceptResponseDto>
                        {
                            ResponseContent = new ClientAcceptResponseDto
                            {
                                Accept = true,
                                AddHeaders = new Dictionary<string, string>
                                {
                                    {"X-Foo", "bar"}
                                },
                                ProvideAccessToken = provideAccessToken,
                                ProvideIdToken = provideIdToken,
                                ProvideRefreshToken = provideRefreshToken
                            },
                            ResponseDump = "[resp-dump]",
                            RequestDump = "[req-dump]"
                        }
                    )
                );

            var client = _fxt.Start
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.AddSingleton(oidcProviderMock);
                    srv.AddSingleton(bizLogicApiMock.Object);
                    AddXUnit(srv);
                }).ApiClient;

            //Act
            var finishResultCallDetails = await client.Call(api => api.FinishAsync(_testAuthCode, _testAuthState, _testCodeVerifier));

            //Assert
            await finishResultCallDetails.ThrowIfUnexpectedStatusCode();
            Assert.NotNull(finishResultCallDetails.ResponseContent);
            Assert.True((provideAccessToken ? _testAccessToken : null) == finishResultCallDetails.ResponseContent.AccessToken);
            Assert.True((provideRefreshToken ? _testRefreshToken : null) == finishResultCallDetails.ResponseContent.RefreshToken);
            Assert.True((provideIdToken ? TestJohnDoeIdToken : null) == finishResultCallDetails.ResponseContent.IdToken);
            Assert.Equal(_testExpiresIn, finishResultCallDetails.ResponseContent.ExpiresIn);

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
            var oidcProviderMock = CreateOidcProviderMock();

            var bizLogicApiMock = new Mock<IBizLogicApi>();
            bizLogicApiMock.Setup(api => api.AcceptDetailedAsync(It.IsAny<ClientAcceptRequestDto>()))
                .ReturnsAsync
                (
                    new Func<ClientAcceptRequestDto, CallDetails<ClientAcceptResponseDto>>
                    (
                        _ => new CallDetails<ClientAcceptResponseDto>
                        {
                            ResponseContent = new ClientAcceptResponseDto
                            {
                                Accept = false,
                                RejectionReason = "test reason"
                            },
                            ResponseDump = "[resp-dump]",
                            RequestDump = "[req-dump]"
                        } 
                    )
                );

            var client = _fxt.Start
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.AddSingleton(oidcProviderMock);
                    srv.AddSingleton(bizLogicApiMock.Object);
                    AddXUnit(srv);
                }).ApiClient;

            //Act
            var finishResultCallDetails = await client.Call(api => api.FinishAsync(_testAuthCode, _testAuthState, _testCodeVerifier));

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
            var oidcProviderMock = CreateOidcProviderMock();

            ClientAcceptRequestDto? acceptRequest = null;

            var bizLogicApiMock = new Mock<IBizLogicApi>();
            bizLogicApiMock.Setup(api => api.AcceptDetailedAsync(It.IsAny<ClientAcceptRequestDto>()))
                .ReturnsAsync
                    (
                        new Func<ClientAcceptRequestDto, CallDetails<ClientAcceptResponseDto>>
                            (
                                req =>
                                {
                                    acceptRequest = req;
                                    return new CallDetails<ClientAcceptResponseDto>
                                    {
                                        ResponseContent = new ClientAcceptResponseDto
                                        {
                                            Accept = true
                                        },
                                        ResponseDump = "[resp-dump]",
                                        RequestDump = "[req-dump]"
                                    };
                                })
                    );

            var proxy = _fxt.StartWithProxy
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.AddSingleton(oidcProviderMock);
                    srv.AddSingleton(bizLogicApiMock.Object);
                    AddXUnit(srv);
                }).ApiClient;
            
            //Act
            await proxy.FinishAsync(_testAuthCode, _testAuthState, _testCodeVerifier);

            //Assert
            Assert.NotNull(acceptRequest);
            Assert.Equal(_testAccessToken, acceptRequest.TokenResponse.AccessToken);
            Assert.Equal(_testRefreshToken, acceptRequest.TokenResponse.RefreshToken);
            Assert.Equal(TestJohnDoeIdToken, acceptRequest.TokenResponse.IdToken);
            Assert.Equal("code",acceptRequest.TokenResponse.TokenType);
            Assert.Equal(_testExpiresIn,acceptRequest.TokenResponse.ExpiresIn);
            Assert.NotNull(acceptRequest.IdToken);
            Assert.Contains(acceptRequest.IdToken, c => c is { Key:"name", Value:"John Doe" });
        }
    }
}