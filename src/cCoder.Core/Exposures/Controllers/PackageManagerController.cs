// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.Core.Services.Aggregations.Packages;
using cCoder.Core.Models.Exceptions;
using cCoder.Data.Models.Packaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace cCoder.Core.Exposures.Controllers;

[ODataIgnored]
[ApiController]
[Route("Api/Core/Package")]
public class PackageManagerController(
    IPackageManager packageManagerAggregationService)
    : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [HttpGet("Export")]
    public async Task<IActionResult> Get(
        [FromQuery] int appId,
        [FromQuery] string[] packageNames = null)
    {
        try
        {
            string sourceApi = $"{Request.Scheme}://{Request.Host}";

            Package[] packages =
                await packageManagerAggregationService.ExportPackagesAsync(
                    appId: appId,
                    packageNames: packageNames,
                    sourceApi: sourceApi);

            return Ok(value: packages);
        }
        catch (CoreOrchestrationValidationException)
        {
            return BadRequest(error: "The package request is invalid.");
        }
        catch (System.Security.SecurityException)
        {
            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The package operation is forbidden.");
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The package operation failed.");
        }
    }

    [HttpPost("Import")]
    public async Task<IActionResult> Post(
        [FromQuery] int appId,
        [FromBody] Package newPackage)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(modelState: ModelState);
            }

            await packageManagerAggregationService.ImportPackagesAsync(
                appId: appId,
                packages: [newPackage]);

            return Ok();
        }
        catch (CoreOrchestrationValidationException)
        {
            return BadRequest(error: "The package request is invalid.");
        }
        catch (System.Security.SecurityException)
        {
            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The package operation is forbidden.");
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The package operation failed.");
        }
    }

    [HttpPost("ImportThis")]
    public async Task<IActionResult> PostThis([FromQuery] int appId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(modelState: ModelState);
            }

            using JsonDocument document =
                await JsonDocument.ParseAsync(utf8Json: Request.Body);

            JsonElement body = document.RootElement;

            Package[] packages = body.ValueKind == JsonValueKind.Array
                ? body.Deserialize<Package[]>(options: JsonOptions) ?? []
                : body.Deserialize<Package>(options: JsonOptions) is Package package
                    ? [package]
                    : [];

            await packageManagerAggregationService.ImportPackagesAsync(
                appId: appId,
                packages: packages);

            return Ok();
        }
        catch (CoreOrchestrationValidationException)
        {
            return BadRequest(error: "The package request is invalid.");
        }
        catch (System.Security.SecurityException)
        {
            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The package operation is forbidden.");
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The package operation failed.");
        }
    }
}