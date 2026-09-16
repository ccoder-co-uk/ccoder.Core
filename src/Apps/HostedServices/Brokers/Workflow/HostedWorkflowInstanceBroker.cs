// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Exposures;

namespace HostedServices.Brokers.Workflow;

internal sealed class HostedWorkflowInstanceBroker(
    IWorkflowInstanceManager workflowInstanceManager)
    : IHostedWorkflowInstanceBroker
{
    public ValueTask ExecuteWaitingQueuedInstanceByIdAsync(Guid flowInstanceDataId) =>
        workflowInstanceManager.ExecuteWaitingQueuedInstanceByIdAsync(
            flowInstanceDataId: flowInstanceDataId);

    public object[] GetStats() =>
        workflowInstanceManager.GetStats();
}