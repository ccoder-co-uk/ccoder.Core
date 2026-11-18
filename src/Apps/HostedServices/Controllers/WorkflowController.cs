// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
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

            return View(viewName: "Index");
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

            return Ok();
        }
        catch (ArgumentException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);

            return BadRequest(error: "The workflow request is invalid.");
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
            return Json(data: workflowInstanceProcessingService.GetStats());
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