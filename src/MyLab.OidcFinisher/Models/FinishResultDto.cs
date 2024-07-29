namespace MyLab.OidcFinisher.Models
{
    public class FinishResultDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? IdToken { get; set; }
    }
}
