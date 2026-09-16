// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
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
        catch (CoreOrchestrationValidationException exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return BadRequest(error: "The package request is invalid.");
        }
        catch (System.Security.SecurityException exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The package operation is forbidden.");
        }
        catch (Exception exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The package operation failed.");
        }
    }

}