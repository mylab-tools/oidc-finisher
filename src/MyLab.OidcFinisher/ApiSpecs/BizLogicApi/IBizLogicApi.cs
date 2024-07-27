using MyLab.ApiClient;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi
{
    [Api]
    public interface IBizLogicApi
    {
        [Post]
        Task<ClientAcceptRequestDto> AcceptAsync([JsonContent]ClientAcceptRequestDto request, CancellationToken cancellationToken);
    }
}
