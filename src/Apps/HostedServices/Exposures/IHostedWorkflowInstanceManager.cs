// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace HostedServices.Exposures;

public interface IHostedWorkflowInstanceManager
{
    ValueTask ExecuteWaitingQueuedInstanceByIdAsync(Guid flowInstanceDataId);
    object[] GetStats();
}