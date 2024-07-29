using MyLab.ApiClient;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi
{
    [Api]
    public interface IBizLogicApi
    {
        [Post]
        Task<ClientAcceptResponseDto> AcceptAsync([JsonContent]ClientAcceptRequestDto request);
    }
}
