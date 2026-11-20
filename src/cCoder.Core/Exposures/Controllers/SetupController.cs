// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using cCoder.Core.Models;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Models.Exceptions;
using cCoder.Core.Brokers.Loggings;

namespace cCoder.Core.Exposures.Controllers;

[Route("Setup")]
public sealed class SetupController(
    IFirstTimeSetupStateOrchestrationService setupStateService,
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
                loggingBroker.LogInformation(
                    message: "The initialized setup request was redirected.");

                return Redirect(url: "/");
            }

            loggingBroker.LogInformation(
                message: "The setup page was loaded successfully.");

            return View(
                viewName: "Index",
                model: CreateFirstTimeSetupViewModel());
        }
        catch (CoreOrchestrationValidationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The setup request is invalid.");

            return BadRequest(error: "The setup request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The setup request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The setup operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The setup operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "The setup operation failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The setup operation failed.");
        }
    }

    private FirstTimeSetupViewModel CreateFirstTimeSetupViewModel() =>
        new()
        {
            AssetsRoot = configuration.Packaging.AssetsRoot,
            Domain = setupStateService.NormalizeHost(
                host: Request.Host.Host),
            Setup = new FirstTimeSetupRequest(),
        };
}