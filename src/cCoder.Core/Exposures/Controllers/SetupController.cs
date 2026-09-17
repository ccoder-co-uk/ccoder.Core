// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using Microsoft.AspNetCore.Mvc;
using cCoder.Core.Models;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Exposures.Controllers;

[Route("Setup")]
public sealed class SetupController(
    IFirstTimeSetupOrchestrationService setupService,
    CoreConfiguration configuration)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            if (await setupService.IsInitializedAsync(
                cancellationToken: cancellationToken))
            {
                return Redirect(url: "/");
            }

            return View(
                viewName: "Index",
                model: CreateFirstTimeSetupViewModel());
        }
        catch (CoreOrchestrationValidationException exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return BadRequest(error: "The setup request is invalid.");
        }
        catch (System.Security.SecurityException exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The setup operation is forbidden.");
        }
        catch (Exception exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The setup operation failed.");
        }
    }

    private FirstTimeSetupViewModel CreateFirstTimeSetupViewModel() =>
        new()
        {
            AssetsRoot = configuration.Packaging.AssetsRoot,
            Domain = setupService.NormalizeHost(
                host: Request.Host.Host),
            Setup = new FirstTimeSetupRequest(),
        };
}