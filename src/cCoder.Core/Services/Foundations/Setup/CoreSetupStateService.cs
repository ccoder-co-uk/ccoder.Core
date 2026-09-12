// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Setup;

namespace cCoder.Core.Services.Foundations.Setup;

internal sealed partial class CoreSetupStateService(
    ICoreSetupContextBroker coreSetupContextBroker)
    : ICoreSetupStateService
{
    public ValueTask<bool> IsCoreInitializedAsync(
        CancellationToken cancellationToken = default) =>
        TryCatch(operation: async () =>
        {
            ValidateCancellationTokenOnCheck(
                cancellationToken: cancellationToken);

            return await coreSetupContextBroker.IsInitializedAsync(
                cancellationToken: cancellationToken);
        });
}