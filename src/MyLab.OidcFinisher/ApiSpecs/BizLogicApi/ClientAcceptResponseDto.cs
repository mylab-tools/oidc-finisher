using System.Text.Json.Serialization;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi;

public class ClientAcceptResponseDto
{
    [JsonPropertyName("accept")]
    public required bool Accept { get; set; }
    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason{ get; set; }
    [JsonPropertyName("provideAccessToken")]
    public bool ProvideAccessToken { get; set; } = true;
    [JsonPropertyName("provideRefreshToken")]
    public bool ProvideRefreshToken { get; set; } = true;
    [JsonPropertyName("provideIdToken")]
    public bool ProvideIdToken { get; set; } = true;
    [JsonPropertyName("addHeaders")]
    public Dictionary<string, string>? AddHeaders { get; set; }
}