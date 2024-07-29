using Microsoft.AspNetCore.Mvc;
using MyLab.ApiClient;
using MyLab.OidcFinisher.Models;

namespace FuncTests;

[Api("oidc")]
public interface IOidcFinisherApi
{
    [Post("finish")]
    Task<FinishResultDto> FinishAsync([Query]string code, [Query]string state);
}