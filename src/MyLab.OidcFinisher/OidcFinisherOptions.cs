using System.ComponentModel.DataAnnotations;

namespace MyLab.OidcFinisher
{
    public class OidcFinisherOptions
    {
        public bool AutoAccept { get; set; } = false;
        [Required]
        public required string ClientId { get; set; }
        [Required]
        public required string ClientSecret { get; set; }
        [Required]
        public required string RedirectUri { get; set; }
    }
}
