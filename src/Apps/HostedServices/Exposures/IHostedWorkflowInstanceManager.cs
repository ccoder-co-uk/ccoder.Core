// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
namespace HostedServices.Exposures;

public interface IHostedWorkflowInstanceManager
{
    ValueTask ExecuteWaitingQueuedInstanceByIdAsync(Guid flowInstanceDataId);
    object[] GetStats();
}