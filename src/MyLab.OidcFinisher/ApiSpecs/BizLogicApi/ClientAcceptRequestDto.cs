using MyLab.OidcFinisher.ApiSpecs.OidcProvider;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi;

public class ClientAcceptRequestDto
{
    [JsonPropertyName("tokenResponse")]
    [JsonProperty("tokenResponse")]
    public required TokenResponseDto TokenResponse { get; set; }
    [JsonPropertyName("idToken")]
    [JsonProperty("idToken")]
    public IReadOnlyDictionary<string, string>? IdToken { get; set; }
}