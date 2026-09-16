// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using HostedServices.Exposures;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("Workflow")]
public sealed class WorkflowController(
    IHostedWorkflowInstanceManager workflowInstanceProcessingService)
    : Controller
{
    [HttpGet("")]
    public IActionResult Get()
    {
        Response.StatusCode = StatusCodes.Status200OK;

        return View(viewName: "Index");
    }

    [HttpPost("ExecuteNextFlowInstanceInQueue")]
    public async Task<IActionResult> Post(Guid flowId)
    {
        await workflowInstanceProcessingService.ExecuteWaitingQueuedInstanceByIdAsync(
            flowInstanceDataId: flowId);

        return Ok();
    }

    [HttpGet("GetStats")]
    public IActionResult GetStats() =>
        Json(data: workflowInstanceProcessingService.GetStats());
}