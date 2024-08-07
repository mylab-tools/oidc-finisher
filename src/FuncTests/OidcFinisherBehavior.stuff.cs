using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.ApiClient;
using MyLab.ApiClient.Test;
using MyLab.Log.XUnit;
using MyLab.OidcFinisher;
using MyLab.OidcFinisher.ApiSpecs.OidcProvider;
using Xunit.Abstractions;

namespace FuncTests;

public partial class OidcFinisherBehavior : IClassFixture<TestApiFixture<Program, IOidcFinisherApiV1>>
{
    private readonly TestApiFixture<Program, IOidcFinisherApiV1> _fxt;
    private readonly ITestOutputHelper _output;

    private const string TestJohnDoeIdToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    private readonly string _testAccessToken;
    private readonly string _testRefreshToken;
    private readonly string _testAuthCode;
    private readonly string _testAuthState;
    private readonly string _testCodeVerifier;
    private readonly Expression<Func<TokenRequestDto, bool>> _tokenRequestPredict;
    private readonly Action<OidcFinisherOptions> _optionsHandler;
    private readonly CallDetails<TokenResponseDto> _testTokenResponseDetailed;

    public OidcFinisherBehavior(TestApiFixture<Program, IOidcFinisherApiV1> fxt, ITestOutputHelper output)
    {
        _fxt = fxt;
        _output = output;

        _testAccessToken = Guid.NewGuid().ToString("N");
        _testRefreshToken = Guid.NewGuid().ToString("N");
        _testAuthCode = Guid.NewGuid().ToString("N");
        _testAuthState = Guid.NewGuid().ToString("N");
        _testCodeVerifier = Guid.NewGuid().ToString("N");

        const string testRedirectUri = "http://foo.bar";

        _optionsHandler = opt =>
        {
            opt.ClientId = "foo";
            opt.ClientSecret = "bar";
            opt.RedirectUri = testRedirectUri;
            opt.AutoAccept = false;
        };

        _tokenRequestPredict = r =>
            r.RedirectUri == testRedirectUri &&
            r.Code == _testAuthCode &&
            r.CodeVerifier == _testCodeVerifier &&
            r.GrantType == TokenRequestDto.AuthorizationCodeGrantType;

        var testTokenResponse = new TokenResponseDto
        {
            IdToken = TestJohnDoeIdToken,
            AccessToken = _testAccessToken,
            RefreshToken = _testRefreshToken,
            ExpiresIn = 100,
            TokenType = "code"
        };

        _testTokenResponseDetailed = new CallDetails<TokenResponseDto>
        {
            ResponseContent = testTokenResponse,
            RequestDump = "[req-dump]",
            ResponseDump = "[resp-dump]"
        };
    }

    private IOidcProvider CreateOidcProviderMock()
    {
        var oidcProviderMock = new Mock<IOidcProvider>();
        oidcProviderMock.Setup(p => p.GetTokenDetailedAsync
            (
                It.Is(_tokenRequestPredict),
                It.IsAny<string>()
            ))
            .ReturnsAsync(_testTokenResponseDetailed);
        return oidcProviderMock.Object;
    }

    private void AddXUnit(IServiceCollection srv)
    {
        srv.AddLogging(c => c.AddFilter(_ => true).AddXUnit(_output));
    }
}