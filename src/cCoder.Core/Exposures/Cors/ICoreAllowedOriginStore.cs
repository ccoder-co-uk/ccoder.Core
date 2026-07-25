// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Exposures.Cors;

public interface ICoreAllowedOriginStore
{
    ValueTask<bool> IsAllowedAsync(string origin);
}