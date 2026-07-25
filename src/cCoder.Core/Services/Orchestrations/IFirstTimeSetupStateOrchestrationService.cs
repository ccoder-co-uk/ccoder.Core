// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Orchestrations;

internal interface IFirstTimeSetupStateOrchestrationService
{
    Task<bool> IsInitializedAsync(
        CancellationToken cancellationToken = default);
}