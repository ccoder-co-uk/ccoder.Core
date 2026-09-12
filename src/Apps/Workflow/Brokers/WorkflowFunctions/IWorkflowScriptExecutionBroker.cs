// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Workflow.Brokers.WorkflowFunctions;

internal interface IWorkflowScriptExecutionBroker
{
    Task<string> ExecuteAsync(string payload, bool useDetails);
}