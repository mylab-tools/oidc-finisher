using MyLab.OidcFinisher.ApiSpecs.OidcProvider;
using System.Text.Json.Serialization;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi;

public class ClientAcceptRequestDto
{
    [JsonPropertyName("tokenResponse")]
    public required TokenResponseDto TokenResponse { get; set; }
    [JsonPropertyName("idToken")]
    public TokenClaims? IdToken { get; set; }
}