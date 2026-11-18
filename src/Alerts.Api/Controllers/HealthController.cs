using Microsoft.AspNetCore.Mvc;

namespace Alerts.Api.Controllers;

[ApiController]
public sealed class HealthController : ControllerBase
{
    [HttpGet("/healthz")]
    public IActionResult Get() => Ok("ok");
}
