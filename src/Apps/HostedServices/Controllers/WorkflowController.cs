// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Exposures;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("Workflow")]
public sealed class WorkflowController(
    IWorkflowInstanceManager workflowInstanceProcessingService,
    ILogger<WorkflowController> log)
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
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The workflow page could not be displayed.");
        }
    }

    [HttpPost("ExecuteNextFlowInstanceInQueue")]
    public async Task<IActionResult> Post(Guid flowId)
    {
        try
        {
            await workflowInstanceProcessingService.ExecuteWaitingQueuedInstanceByIdAsync(
                flowInstanceDataId: flowId);
        }
        catch (Exception ex)
        {
            log.LogError(
                exception: ex,
                message: "Workflow execution failed: {ErrorMessage}",
                args: ex.Message);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The workflow execution failed.");
        }

        return Ok();
    }

    [HttpGet("GetStats")]
    public IActionResult GetStats()
    {
        try
        {
            return Json(
                data: workflowInstanceProcessingService.GetStats());
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The workflow statistics could not be loaded.");
        }
    }
}