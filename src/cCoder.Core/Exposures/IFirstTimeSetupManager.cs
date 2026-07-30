// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Exposures;

public interface IFirstTimeSetupManager
{
    Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default);
}