using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("Health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Content("OK", "text/plain");
}
