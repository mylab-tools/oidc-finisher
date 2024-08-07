﻿using MyLab.ApiClient;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi
{
    [Api]
    public interface IBizLogicApi
    {
        [Post]
        Task<CallDetails<ClientAcceptResponseDto>> AcceptDetailedAsync([JsonContent] ClientAcceptRequestDto request);
    }
}
