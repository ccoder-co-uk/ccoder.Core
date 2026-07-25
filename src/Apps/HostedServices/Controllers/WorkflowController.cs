// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Services.Processings;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("Workflow")]
public sealed class WorkflowController(
    IWorkflowInstanceProcessingService workflowInstanceProcessingService,
    ILogger<WorkflowController> log)
    : Controller
{
    [HttpGet("")]
    public IActionResult Get() =>
        View(viewName: "Index");

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
            log.LogError(exception: ex,message: ex.Message);

            if (ex.InnerException is not null)
            {
                log.LogError(exception: ex.InnerException,message: ex.InnerException.Message);
            }
        }

        return Ok();
    }

    [HttpGet("GetStats")]
    public IActionResult GetStats() =>
        Json(data: workflowInstanceProcessingService.GetStats());
}