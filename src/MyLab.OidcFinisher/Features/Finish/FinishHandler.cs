using MediatR;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.OidcFinisher.ApiSpecs.BizLogicApi;
using MyLab.OidcFinisher.ApiSpecs.OidcProvider;
using System.Text;

namespace MyLab.OidcFinisher.Features.Finish
{
    public class FinishHandler
        (
            IOptions<OidcFinisherOptions> options,
            IOidcProvider oidcProvider,
            IBizLogicApi? bizLogicApi = null,
            ILogger<FinishHandler>? l = null
        ) : IRequestHandler<FinishCmd, FinishResult>
    {
        private readonly IDslLogger? _log = l?.Dsl();
        private readonly OidcFinisherOptions _opts = options.Value;

        public async Task<FinishResult> Handle(FinishCmd request, CancellationToken cancellationToken)
        {
            var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_opts.ClientId + ":" + _opts.ClientSecret));

            var tokenResponseDetailed = await oidcProvider.GetTokenDetailedAsync
            (
                new TokenRequestDto
                {
                    Code = request.AuthorizationCode,
                    GrantType = TokenRequestDto.AuthorizationCodeGrantType,
                    RedirectUri = _opts.RedirectUri
                },
                "Basic " + svcCredentials
            );

            await tokenResponseDetailed.ThrowIfUnexpectedStatusCode();

            _log?.Debug("Oidc provider request")
                .AndFactIs("request", tokenResponseDetailed.RequestDump)
                .AndFactIs("response", tokenResponseDetailed.ResponseDump)
                .Write();

            var tokenResponse = tokenResponseDetailed.ResponseContent;

            if (_opts.AutoAccept)
            {
                _log?.Debug("Skip Biz logic accept due to AutoAccept mode").Write();

                return new FinishResult
                {
                    Accept = true,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    IdToken = tokenResponse.IdToken
                };
            }

            if (bizLogicApi == null)
                throw new InvalidOperationException("Biz Logic API must be specified when no AutoAccept");

            var acceptResultDetailed = await bizLogicApi.AcceptDetailedAsync
            (
                new ClientAcceptRequestDto
                {
                    TokenResponse = tokenResponse,
                    IdToken = tokenResponse.IdToken != null
                        ? TokenClaims.FromToken(tokenResponse.IdToken)
                        : null
                }
            );

            await acceptResultDetailed.ThrowIfUnexpectedStatusCode();

            _log?.Debug("Biz API request")
                .AndFactIs("request", acceptResultDetailed.RequestDump)
                .AndFactIs("response", acceptResultDetailed.ResponseDump)
                .Write();

            var acceptResult = acceptResultDetailed.ResponseContent;

            if (!acceptResult.Accept)
            {
                _log?.Warning("Authorization rejected!")
                    .AndFactIs("request", request)
                    .AndFactIs("reason", acceptResult.RejectionReason)
                    .Write();
            }

            var fRes = new FinishResult
            {
                Accept = acceptResult.Accept,
                RejectionReason = acceptResult.RejectionReason,
                AccessToken = acceptResult is { Accept: true, ProvideAccessToken: true } 
                    ? tokenResponse.AccessToken : null,
                RefreshToken = acceptResult is { Accept: true, ProvideRefreshToken: true } 
                    ? tokenResponse.RefreshToken : null,
                IdToken = acceptResult is { Accept: true, ProvideIdToken: true } 
                    ? tokenResponse.IdToken : null,
                AdditionHeaders = acceptResult.AddHeaders
            };

            _log?.Debug("Oidc finish result")
                .AndFactIs("result", fRes)
                .Write();

            return fRes;
        }
    }
}
