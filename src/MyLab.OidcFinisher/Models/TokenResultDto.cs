using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MyLab.OidcFinisher.Models
{
    public class TokenResultDto
    {
        [JsonProperty("access_token")]
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonProperty("id_token")]
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }
}
