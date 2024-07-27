using Microsoft.AspNetCore.Mvc;

namespace MyLab.OidcFinisher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OidcController : ControllerBase
    {
        [HttpPost("finish")]
        public Task<IActionResult> FinishAsync([FromQuery] string code, [FromQuery] string? state, CancellationToken cancellationToken)
        {

        }
    }
}
