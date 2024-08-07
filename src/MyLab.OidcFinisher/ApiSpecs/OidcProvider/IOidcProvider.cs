using MyLab.ApiClient;

namespace MyLab.OidcFinisher.ApiSpecs.OidcProvider
{
    [Api]
    public interface IOidcProvider
    {
        [Post("token")]
        public Task<CallDetails<TokenResponseDto>> GetTokenDetailedAsync([FormContent] TokenRequestDto request, [Header("Authorization")] string authorization);
    }
}
