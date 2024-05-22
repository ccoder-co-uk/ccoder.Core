using HostedServices.Services.Scheduled.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("Workflow")]
public sealed class WorkflowController(IWorkflowInstanceManagement workflowInstanceManagementService, ILogger<WorkflowController> log)
    : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpPost("ExecuteNextFlowInstanceInQueue")]
    public async Task<IActionResult> ExecuteNextFlowInstanceInQueue(Guid flowId)
    {
        try
        {
            await workflowInstanceManagementService.ExecuteWaitingQueuedInstanceById(flowId);
        }
        catch (Exception ex)
        {
            log.LogError(ex, ex.Message);

            if (ex.InnerException is not null)
                log.LogError(ex.InnerException, ex.InnerException.Message);
        }

        return Ok();
    }

    [HttpGet("GetStats")]
    public IActionResult GetStats() => Json(workflowInstanceManagementService.GetStats());
}