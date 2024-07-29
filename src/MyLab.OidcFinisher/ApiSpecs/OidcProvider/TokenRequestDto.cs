using MyLab.ApiClient;

namespace MyLab.OidcFinisher.ApiSpecs.OidcProvider;

public class TokenRequestDto
{
    public const string AuthorizationCodeGrantType = "authorization_code";

    [UrlFormItem(Name = "code")]
    public required string Code { get; init; }
    [UrlFormItem(Name = "client_id")]
    public required string ClientId { get; init; }
    [UrlFormItem(Name = "client_secret")]
    public required string ClientSecret { get; init; }
    [UrlFormItem(Name = "redirect_uri")]
    public required string RedirectUri { get; init; }
    [UrlFormItem(Name = "grant_type")]
    public required string GrantType { get; init; } = AuthorizationCodeGrantType;
}