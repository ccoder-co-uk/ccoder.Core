// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Processings.AllowedOrigins;

internal interface IAllowedOriginStoreProcessingService
{
    ValueTask<bool> IsCoreAllowedOriginAllowedAsync(string origin);
}