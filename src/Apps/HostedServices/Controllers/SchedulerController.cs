using HostedServices.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("Scheduler")]
public sealed class SchedulerController(IMigrationService migrationService) : Controller
{
    [HttpPost("Migrate")]
    public IActionResult Migrate() => Ok(migrationService.Migrate());
}