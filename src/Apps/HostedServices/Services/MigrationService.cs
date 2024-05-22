using cCoder.Core.Objects;
using cCoder.Security.Data.EF.Interfaces;
using HostedServices.Services.Interfaces;

namespace HostedServices.Services;

public class MigrationService(
    ISecurityDbContextFactory ssoFactory,
    ICoreDataContext core,
    ILogger<MigrationService> log)
    : IMigrationService
{
    public string Migrate()
    {
        try
        {
            using var sso = ssoFactory.CreateDbContext();

            log.LogInformation("Database Migration Starting ...");

            sso.Migrate();
            core.Migrate();

            log.LogInformation("Database Migration Complete!");
            return "Migration Complete!";
        }
        catch (Exception ex)
        {
            log.LogError($"Migration Failure:\n{ex.Message}\n{ex.StackTrace}");
            return "Migration Failed!";
        }
    }
}