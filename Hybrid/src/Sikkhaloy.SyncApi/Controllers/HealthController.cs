using Microsoft.AspNetCore.Mvc;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
