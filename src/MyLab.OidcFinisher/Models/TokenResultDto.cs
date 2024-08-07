using System.Text.Json.Serialization;

namespace MyLab.OidcFinisher.Models
{
    public class TokenResultDto
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }
        [JsonPropertyName("idToken")]
        public string? IdToken { get; set; }
    }
}
