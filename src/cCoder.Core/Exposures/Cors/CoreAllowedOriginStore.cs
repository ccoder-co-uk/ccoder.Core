// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.AllowedOrigins;

namespace cCoder.Core.Exposures.Cors;

internal sealed class CoreAllowedOriginStore(
    IAllowedOriginStoreProcessingService allowedOriginStoreProcessingService,
    ILogger<CoreAllowedOriginStore> logger)
    : ICoreAllowedOriginStore
{
    public async ValueTask<bool> IsAllowedAsync(string origin)
    {
        try
        {
            return await allowedOriginStoreProcessingService
                .IsCoreAllowedOriginAllowedAsync(origin: origin);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
message: "Unable to resolve the request allowed origins. {Message}", args: exception.Message);

            return false;
        }
    }
}