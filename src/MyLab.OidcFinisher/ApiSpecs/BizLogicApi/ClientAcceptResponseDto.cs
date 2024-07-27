namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi;

public class ClientAcceptResponseDto
{
    public required bool Accept { get; set; }
    public string? RejectionReason{ get; set; }
    public bool ProvideAccessToken { get; set; } = true;
    public bool ProvideRefreshToken { get; set; } = true;
    public bool ProvideIdToken { get; set; } = true;
    public Dictionary<string, string>? AddHeaders { get; set; }
}