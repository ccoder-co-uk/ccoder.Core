// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Workflow.Exposures;

namespace Workflow.Exposures;

public sealed class ExecuteFromServiceBus(
    IWorkflowFunctionsManager workflowFunctionsProcessingService)
{
    public Task RunAsync(string message) =>
        workflowFunctionsProcessingService.ProcessServiceBusMessageAsync(
            message: message);
}