// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using cCoder.Logging.Brokers.Loggings;
using HostedServices.Exposures;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("Workflow")]
public sealed class WorkflowController(
    IHostedWorkflowInstanceManager workflowInstanceProcessingService,
    ILoggingBroker loggingBroker)
    : Controller
{
    [HttpGet("")]
    public IActionResult Get()
    {
        try
        {
            Response.StatusCode = StatusCodes.Status200OK;

            loggingBroker.LogInformation(message: "The workflow page was loaded successfully.");

            return View(viewName: "Index");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);

            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);

            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "Controller request failed.");

            return StatusCode(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("ExecuteNextFlowInstanceInQueue")]
    public async Task<IActionResult> Post(Guid flowId)
    {
        try
        {
            await workflowInstanceProcessingService.ExecuteWaitingQueuedInstanceByIdAsync(
                flowInstanceDataId: flowId);

            loggingBroker.LogInformation(message: "The workflow instance executed successfully.");

            return Ok();
        }
        catch (ArgumentException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);

            return BadRequest(error: "The workflow request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);

            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);

            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "Controller request failed.");

            return StatusCode(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetStats")]
    public IActionResult GetStats()
    {
        try
        {
            object[] stats = workflowInstanceProcessingService.GetStats();

            loggingBroker.LogInformation(message: "The workflow statistics loaded successfully.");

            return Json(data: stats);
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);

            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);

            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "Controller request failed.");

            return StatusCode(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}