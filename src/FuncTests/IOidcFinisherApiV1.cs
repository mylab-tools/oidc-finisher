using Microsoft.AspNetCore.Mvc;
using MyLab.ApiClient;
using MyLab.OidcFinisher.Models;

namespace FuncTests;

[Api("v1/oidc")]
public interface IOidcFinisherApiV1
{
    [Post("finish")]
    Task<TokenResultDto> FinishAsync([Query]string code, [Query]string? state, [Query("code_verifier")] string? codeVerifier);
}