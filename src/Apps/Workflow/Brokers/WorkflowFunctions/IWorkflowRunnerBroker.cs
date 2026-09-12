// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;

namespace Workflow.Brokers.WorkflowFunctions;

internal interface IWorkflowRunnerBroker
{
    Task RunWorkflowRequestAsync(WorkflowRequest workflowRequest);
}