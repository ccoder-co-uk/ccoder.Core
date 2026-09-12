// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Brokers.Setup;

internal interface ISecuritySetupContextBroker
{
    ValueTask<bool> IsInitializedAsync(CancellationToken cancellationToken);
}