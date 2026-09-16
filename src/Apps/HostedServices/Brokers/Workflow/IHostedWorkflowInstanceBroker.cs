// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace HostedServices.Brokers.Workflow;

public interface IHostedWorkflowInstanceBroker
{
    ValueTask ExecuteWaitingQueuedInstanceByIdAsync(Guid flowInstanceDataId);
    object[] GetStats();
}