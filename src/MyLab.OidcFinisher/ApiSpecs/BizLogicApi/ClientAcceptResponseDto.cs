using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi;

public class ClientAcceptResponseDto
{
    [JsonPropertyName("accept")]
    [JsonProperty("accept")]
    public required bool Accept { get; set; }
    [JsonPropertyName("rejectionReason")]
    [JsonProperty("rejectionReason")]
    public string? RejectionReason{ get; set; }
    [JsonPropertyName("provideAccessToken")]
    [JsonProperty("provideAccessToken")]
    public bool ProvideAccessToken { get; set; } = true;
    [JsonPropertyName("provideRefreshToken")]
    [JsonProperty("provideRefreshToken")]
    public bool ProvideRefreshToken { get; set; } = true;
    [JsonPropertyName("provideIdToken")]
    [JsonProperty("provideIdToken")]
    public bool ProvideIdToken { get; set; } = true;
    [JsonPropertyName("addHeaders")]
    [JsonProperty("addHeaders")]
    public Dictionary<string, string>? AddHeaders { get; set; }
}