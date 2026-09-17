// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.Setup;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class FirstTimeSetupOrchestrationService(
    ICoreSetupStateService coreSetupStateService,
    ISecuritySetupStateService securitySetupStateService,
    ISetupRequestHostService setupRequestHostService)
    : IFirstTimeSetupOrchestrationService
{
    public Task<bool> IsInitializedAsync(
        CancellationToken cancellationToken = default) =>
        TryCatch(operation: async () =>
        {
            ValidateCancellationTokenOnCheck(
                cancellationToken: cancellationToken);

            bool coreIsInitialized =
                await coreSetupStateService.IsCoreInitializedAsync(
                    cancellationToken: cancellationToken);

            if (!coreIsInitialized)
            {
                return false;
            }

            return await securitySetupStateService.IsSecurityInitializedAsync(
                cancellationToken: cancellationToken);
        });

    public string NormalizeHost(string host) =>
        TryCatch(operation: () =>
        {
            ValidateHostOnNormalize(host: host);

            return setupRequestHostService.NormalizeHost(host: host);
        });
}