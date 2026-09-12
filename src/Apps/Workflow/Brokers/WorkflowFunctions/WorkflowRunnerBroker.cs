// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Engine.Exposures;

namespace Workflow.Brokers.WorkflowFunctions;

internal sealed class WorkflowRunnerBroker(IFlowRunner flowRunner)
    : IWorkflowRunnerBroker
{
    public Task RunWorkflowRequestAsync(WorkflowRequest workflowRequest) =>
        flowRunner.RunAsync(request: workflowRequest);
}