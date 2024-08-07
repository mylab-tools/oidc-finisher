namespace MyLab.OidcFinisher.Features.Finish;

public class FinishResult
{
    public required bool Accept { get; init; }
    public string? RejectionReason { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? IdToken { get; init; }
    public uint ExpiresIn { get; init; }
    public IDictionary<string,string>? AdditionHeaders { get; init; }
}