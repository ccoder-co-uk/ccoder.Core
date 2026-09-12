// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Setup;
using cCoder.Data;

namespace cCoder.Core.Brokers.Setup;

internal sealed class CoreSetupContextBroker(
    ICoreContextFactory coreContextFactory)
    : ICoreSetupContextBroker
{
    public ValueTask<bool> IsInitializedAsync(
        CancellationToken cancellationToken) =>
        SetupStateDependency.IsCoreInitializedAsync(
            coreContextFactory: coreContextFactory,
            cancellationToken: cancellationToken);
}