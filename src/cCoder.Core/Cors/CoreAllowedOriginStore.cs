using cCoder.Data;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Cors;

public sealed class CoreAllowedOriginStore(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<CoreAllowedOriginStore> logger)
    : ICoreAllowedOriginStore
{
    private CoreAllowedOriginSnapshot snapshot = CoreAllowedOriginPolicy.CreateSnapshot([]);

    public CoreAllowedOriginSnapshot Snapshot => snapshot;

    public bool IsAllowed(string origin) =>
        CoreAllowedOriginPolicy.IsAllowed(origin, snapshot);

    public async Task RefreshAsync()
    {
        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            ICoreContextFactory coreContextFactory =
                scope.ServiceProvider.GetRequiredService<ICoreContextFactory>();
            using CoreDataContext context = coreContextFactory.CreateCoreContext();

            string[] configuredOrigins = await context.Apps
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(app => !string.IsNullOrWhiteSpace(app.Domain))
                .Select(app => app.Domain)
                .ToArrayAsync();

            snapshot = CoreAllowedOriginPolicy.CreateSnapshot(configuredOrigins);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Unable to refresh the core allowed-origin cache. {Message}",
                exception.Message);
        }
    }
}
