using System.Text.Json.Serialization;

namespace MyLab.OidcFinisher.ApiSpecs.OidcProvider;

public class TokenResponseDto
{
    [JsonPropertyName("accessToken")]
    public required string AccessToken { get; set; }
    [JsonPropertyName("expiresIn")]
    public uint ExpiresIn{ get; set; }
    [JsonPropertyName("idToken")]
    public string? IdToken{ get; set; }
    [JsonPropertyName("tokenType")]
    public required string TokenType{ get; set; }
    [JsonPropertyName("refreshToken")]
    public string? RefreshToken{ get; set; }
}