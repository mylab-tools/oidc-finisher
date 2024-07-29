using MediatR;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.OidcFinisher.ApiSpecs.BizLogicApi;
using MyLab.OidcFinisher.ApiSpecs.OidcProvider;

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
            var tokenResponse = await oidcProvider.GetTokenAsync
            (
                new TokenRequestDto
                {
                    ClientId = _opts.ClientId,
                    ClientSecret = _opts.ClientSecret,
                    Code = request.AuthorizationCode,
                    GrantType = TokenRequestDto.AuthorizationCodeGrantType,
                    RedirectUri = _opts.RedirectUri
                }
            );

            if (_opts.AutoAccept)
            {
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

            var acceptResult = await bizLogicApi.AcceptAsync
            (
                new ClientAcceptRequestDto
                {
                    TokenResponse = tokenResponse,
                    IdToken = tokenResponse.IdToken != null
                        ? TokenClaims.FromToken(tokenResponse.IdToken)
                        : null
                }
            );

            if (!acceptResult.Accept)
            {
                _log?.Warning("Authorization rejected!")
                    .AndFactIs("iodc", request)
                    .AndFactIs("reason", acceptResult.RejectionReason)
                    .Write();
            }

            return new FinishResult
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
        }
    }
}
