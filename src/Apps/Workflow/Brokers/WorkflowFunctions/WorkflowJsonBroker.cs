// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using Workflow.Dependencies;

namespace Workflow.Brokers.WorkflowFunctions;

internal sealed class WorkflowJsonBroker(
    WorkflowJsonDependency workflowJsonDependency)
        : IWorkflowJsonBroker
{
    public WorkflowRequest DeserializeWorkflowRequest(string json) =>
        workflowJsonDependency.DeserializeWorkflowRequest(json: json);
}