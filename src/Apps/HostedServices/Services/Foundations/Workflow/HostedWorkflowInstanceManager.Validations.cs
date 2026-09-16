// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace HostedServices.Services.Foundations.Workflow;

internal sealed partial class HostedWorkflowInstanceManager
{
    private static void ValidateExecuteWaitingQueuedInstanceOnExecute(
        Guid flowInstanceDataId) =>
        Validate(inputs: [flowInstanceDataId]);

    private static void ValidateStatsOnGet() =>
        Validate(inputs: [nameof(GetStats)]);

    private static void Validate(params object[] inputs)
    {
        _ = inputs;
    }
}