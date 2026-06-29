using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("Health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Content("OK", "text/plain");
}
