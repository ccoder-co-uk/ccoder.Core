using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Core.Services.Processings.AllowedOrigins;

namespace cCoder.Core.Exposures.Cors;

internal sealed class CoreAllowedOriginStore(
    IAllowedOriginStoreService allowedOriginStoreService,
    IAllowedOriginProcessingService allowedOriginProcessingService,
    ILogger<CoreAllowedOriginStore> logger)
    : ICoreAllowedOriginStore
{
    public async ValueTask<bool> IsAllowedAsync(string origin)
    {
        try
        {
            string[] configuredOrigins =
                await allowedOriginStoreService.GetAllowedOriginsAsync();

            return allowedOriginProcessingService.IsAllowed(
                origin,
                allowedOriginProcessingService.CreateSnapshot(configuredOrigins));
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Unable to resolve the request allowed origins. {Message}",
                exception.Message);

            return false;
        }
    }
}
