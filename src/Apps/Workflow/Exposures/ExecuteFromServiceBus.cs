// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Workflow.Services.Processings.WorkflowFunctions;

namespace Workflow.Exposures;

public sealed class ExecuteFromServiceBus(
    IWorkflowFunctionsProcessingService workflowFunctionsProcessingService)
{
    public Task RunAsync(string message) =>
        workflowFunctionsProcessingService.ProcessServiceBusMessageAsync(
            message: message);
}