using FuncTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient;
using MyLab.ApiClient.Test;
using MyLab.Log.XUnit;
using MyLab.OidcFinisher;
using Xunit.Abstractions;

namespace IntegrationTests;

public partial class OidcFinisherBehavior : IClassFixture<TestApiFixture<Program, IOidcFinisherApiV1>>
{
    private readonly TestApiFixture<Program, IOidcFinisherApiV1> _fxt;
    private readonly ITestOutputHelper _output;
    private readonly Action<OidcFinisherOptions> _optionsHandler;
    private const string TestAuthCode = "foo-code";
    private const string TestAuthState = "foo-state";
    private const string TestCodeVerifier = "foo-verifier";
    private const string TestAccessToken = "SlAV32hkKG";
    private const string TestRefreshToken = "8xLOxBtZp8";

    private const string TestIdToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    private const uint TestExpiresIn = 3600;

    public OidcFinisherBehavior(TestApiFixture<Program, IOidcFinisherApiV1> fxt, ITestOutputHelper output)
    {
        _fxt = fxt;
        _output = output;

        _optionsHandler = opt =>
        {
            opt.ClientId = "foo";
            opt.ClientSecret = "bar";
            opt.RedirectUri = "http://foo.bar";
            opt.AutoAccept = false;
        };
    }

    private void AddXUnit(IServiceCollection srv)
    {
        srv.AddLogging(c => c.AddFilter(_ => true).AddXUnit(_output));
    }

    private void TuneApiClients(IServiceCollection srv, string? bizAppEndpoint)
    {
        var connectionOptions = new Dictionary<string, ApiConnectionOptions>
        {
            {
                "oidc",
                new ApiConnectionOptions
                {
                    Url = "http://localhost:8085/oidc"
                }
            }
        };

        if (bizAppEndpoint != null)
        {
            connectionOptions.Add
            (
                "app",
                new ApiConnectionOptions
                {
                    Url = new Uri(new Uri("http://localhost:8085/app/"), bizAppEndpoint).AbsoluteUri.TrimEnd('/')
                }
            );
        }

        srv.ConfigureApiClients(opt => { opt.List = connectionOptions; });
    }
}