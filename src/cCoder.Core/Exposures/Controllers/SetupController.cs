// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using Microsoft.AspNetCore.Mvc;
using cCoder.Core.Models;
using cCoder.Core.Services.Setup;
using cCoder.Core.Exposures.Setup;
using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Exposures.Controllers;

[Route("Setup")]
public sealed class SetupController(
    IFirstTimeSetupManager setupStateService,
    ISetupRequestHostManager setupRequestHostManager,
    CoreConfiguration configuration,
    ILoggingBroker loggingBroker)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            if (await setupStateService.IsInitializedAsync(
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
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return BadRequest(error: "The setup request is invalid.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The setup operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The setup operation failed.");
        }
    }

    private FirstTimeSetupViewModel CreateFirstTimeSetupViewModel() =>
        new()
        {
            AssetsRoot = configuration.Packaging.AssetsRoot,
            Domain = setupRequestHostManager.NormalizeHost(
                host: Request.Host.Host),
            Setup = new FirstTimeSetupRequest(),
        };
}