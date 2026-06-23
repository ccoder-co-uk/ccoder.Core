using cCoder.Workflow.Activities.Models;
using Microsoft.Extensions.Logging;

namespace Workflow.Services;

public sealed class WorkflowExecutionService(
    FlowRunner flowRunner,
    ILogger<WorkflowExecutionService> logger)
{
    public async Task ExecuteAsync(WorkflowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await flowRunner.RunAsync(request);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Workflow execution failed for instance {InstanceId}.", request.InstanceId);
            throw;
        }
    }
}
