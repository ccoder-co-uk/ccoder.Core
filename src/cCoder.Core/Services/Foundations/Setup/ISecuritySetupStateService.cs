// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Foundations.Setup;

internal interface ISecuritySetupStateService
{
    ValueTask<bool> IsSecurityInitializedAsync(
        CancellationToken cancellationToken = default);
}