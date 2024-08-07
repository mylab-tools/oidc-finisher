using System.Text.Json.Serialization;

namespace MyLab.OidcFinisher.Models
{
    public class TokenResultDto
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }
}
