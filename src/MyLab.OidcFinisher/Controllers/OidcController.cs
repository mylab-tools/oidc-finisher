using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyLab.OidcFinisher.Features.Finish;
using MyLab.OidcFinisher.Models;

namespace MyLab.OidcFinisher.Controllers
{
    [ApiController]
    [Route("v1/oidc")]
    public class OidcController(IMediator mediator) : ControllerBase
    {
        [HttpPost("finish")]
        public async Task<IActionResult> FinishAsync
        (
            [FromQuery] string code, 
            [FromQuery] string? state, 
            [FromQuery(Name = "code_verifier")] string? codeVerifier, 
            CancellationToken cancellationToken
        )
        {
            var finishResult = await mediator.Send(new FinishCmd(code, state, codeVerifier), cancellationToken);

            if (!finishResult.Accept)
                return StatusCode((int)HttpStatusCode.Forbidden, finishResult.RejectionReason);

            var finishResultDto = new FinishResultDto
            {
                IdToken = finishResult.IdToken,
                AccessToken = finishResult.AccessToken,
                RefreshToken = finishResult.RefreshToken
            };

            if (finishResult.AdditionHeaders is { Count: > 0 })
            {
                foreach (var finishResultAdditionHeader in finishResult.AdditionHeaders)
                {
                    Response.Headers.Append
                    (
                        finishResultAdditionHeader.Key, 
                        finishResultAdditionHeader.Value
                    );
                }
            }

            return Ok(finishResultDto);
        }
    }
}
