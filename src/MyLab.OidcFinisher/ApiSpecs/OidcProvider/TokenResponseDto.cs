using System.Text.Json.Serialization;

namespace MyLab.OidcFinisher.ApiSpecs.OidcProvider;

public class TokenResponseDto
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }
    [JsonPropertyName("expires_in")]
    public uint ExpiresIn{ get; set; }
    [JsonPropertyName("id_token")]
    public string? IdToken{ get; set; }
    [JsonPropertyName("token_type")]
    public required string TokenType{ get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken{ get; set; }
}