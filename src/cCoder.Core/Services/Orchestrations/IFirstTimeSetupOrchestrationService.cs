// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Orchestrations;

public interface IFirstTimeSetupOrchestrationService
{
    Task<bool> IsInitializedAsync(
        CancellationToken cancellationToken = default);

    string NormalizeHost(string host);
}