using System.Text.Json;
using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using cCoder.Packaging.Services.Orchestrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace cCoder.Core.Api.Controllers;

[ODataIgnored]
[ApiController]
[Route("Api/Core/Package")]
public class PackageManagerController(
    IPackageManagerOrchestrationService packageManagerOrchestrationService
) : ControllerBase
{
    private static readonly string[] DefaultPackageNames =
    [
        "Roles",
        "Layouts",
        "Templates",
        "Resources",
        "Pages",
        "Workflows",
        "Components",
        "Scripts",
        "PageRoles",
        "FolderRoles",
        "Calendars",
        "CalendarEvents",
    ];

    [HttpGet("Export")]
    public IActionResult Export([FromQuery] int appId, [FromQuery] string[] packageNames = null)
    {
        string[] requestedPackages =
            packageNames?.Where(packageName => !string.IsNullOrWhiteSpace(packageName)).ToArray()
            ?? [];

        if (requestedPackages.Length == 0)
            requestedPackages = DefaultPackageNames;

        return Ok(
            requestedPackages.Select(packageName =>
                packageManagerOrchestrationService.ExportPackage(appId, packageName))
        );
    }

    [HttpPost("Import")]
    public async Task<IActionResult> ImportAsync([FromQuery] int appId, [FromBody] Package package)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await packageManagerOrchestrationService.ImportPackageAsync(appId, package);
        return Ok();
    }

    [HttpPost("ImportThis")]
    public async Task<IActionResult> ImportThisAsync([FromQuery] int appId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using StreamReader reader = new(Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        JsonElement body = JsonDocument.Parse(requestBody).RootElement;

        if (body.ValueKind == JsonValueKind.Array)
        {
            Package[] packages = body.Deserialize<Package[]>();

            foreach (Package package in packages ?? [])
                await packageManagerOrchestrationService.ImportPackageAsync(appId, package);

            return Ok();
        }

        Package entity = body.Deserialize<Package>();
        if (entity is not null)
            await packageManagerOrchestrationService.ImportPackageAsync(appId, entity);

        return Ok();
    }
}
