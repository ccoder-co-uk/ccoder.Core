// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Setup;

namespace cCoder.Core.Services.Foundations.Setup;

internal sealed partial class SecuritySetupStateService(
    ISecuritySetupContextBroker securitySetupContextBroker)
    : ISecuritySetupStateService
{
    public ValueTask<bool> IsSecurityInitializedAsync(
        CancellationToken cancellationToken = default) =>
        TryCatch(operation: async () =>
        {
            ValidateCancellationTokenOnCheck(
                cancellationToken: cancellationToken);

            return await securitySetupContextBroker.IsInitializedAsync(
                cancellationToken: cancellationToken);
        });
}