// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Foundations.Setup;

internal interface ICoreSetupStateService
{
    ValueTask<bool> IsCoreInitializedAsync(
        CancellationToken cancellationToken = default);
}