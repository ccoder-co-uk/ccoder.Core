// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
namespace HostedServices.Brokers.Workflow;

public interface IHostedWorkflowInstanceBroker
{
    ValueTask ExecuteWaitingQueuedInstanceByIdAsync(Guid flowInstanceDataId);
    object[] GetStats();
}