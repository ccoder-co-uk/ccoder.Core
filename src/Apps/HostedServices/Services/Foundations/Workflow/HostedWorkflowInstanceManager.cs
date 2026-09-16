// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using HostedServices.Brokers.Workflow;
using HostedServices.Exposures;

namespace HostedServices.Services.Foundations.Workflow;

internal sealed partial class HostedWorkflowInstanceManager(
    IHostedWorkflowInstanceBroker hostedWorkflowInstanceBroker)
    : IHostedWorkflowInstanceManager
{
    public ValueTask ExecuteWaitingQueuedInstanceByIdAsync(Guid flowInstanceDataId) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateExecuteWaitingQueuedInstanceOnExecute(
                flowInstanceDataId: flowInstanceDataId);

            await hostedWorkflowInstanceBroker
                .ExecuteWaitingQueuedInstanceByIdAsync(
                    flowInstanceDataId: flowInstanceDataId);
        });

    public object[] GetStats() =>
        TryCatch(operation: () =>
        {
            ValidateStatsOnGet();

            return hostedWorkflowInstanceBroker.GetStats();
        });
}