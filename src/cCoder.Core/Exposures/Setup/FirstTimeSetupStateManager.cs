// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Services.Setup;

namespace cCoder.Core.Exposures.Setup;

internal sealed class FirstTimeSetupStateManager(
    IFirstTimeSetupStateOrchestrationService
        firstTimeSetupStateOrchestrationService)
    : IFirstTimeSetupStateService
{
    public Task<bool> IsInitializedAsync(
        CancellationToken cancellationToken = default) =>
        firstTimeSetupStateOrchestrationService.IsInitializedAsync(
            cancellationToken: cancellationToken);
}