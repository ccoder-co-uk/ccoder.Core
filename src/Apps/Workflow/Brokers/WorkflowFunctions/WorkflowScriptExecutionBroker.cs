// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Engine.Exposures;

namespace Workflow.Brokers.WorkflowFunctions;

internal sealed class WorkflowScriptExecutionBroker(
    IWorkflowScriptExecutionService workflowScriptExecutionService)
        : IWorkflowScriptExecutionBroker
{
    public Task<string> ExecuteAsync(string payload, bool useDetails) =>
        workflowScriptExecutionService.ExecuteAsync(
            payload: payload,
            useDetails: useDetails);
}