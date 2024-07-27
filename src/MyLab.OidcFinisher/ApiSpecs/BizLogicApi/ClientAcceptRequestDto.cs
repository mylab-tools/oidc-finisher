using MyLab.OidcFinisher.ApiSpecs.OidcProvider;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi;

public class ClientAcceptRequestDto
{
    public required TokenResponseDto TokenResponse { get; set; }
    public TokenClaims? IdToken { get; set; }
}