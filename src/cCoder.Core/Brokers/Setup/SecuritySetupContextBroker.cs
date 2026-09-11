// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Setup;
using cCoder.Security.Data.EF.Interfaces;

namespace cCoder.Core.Brokers.Setup;

internal sealed class SecuritySetupContextBroker(
    ISecurityDbContextFactory securityDbContextFactory)
    : ISecuritySetupContextBroker
{
    public ValueTask<bool> IsInitializedAsync(
        CancellationToken cancellationToken) =>
        SetupStateDependency.IsSecurityInitializedAsync(
            securityDbContextFactory: securityDbContextFactory,
            cancellationToken: cancellationToken);
}