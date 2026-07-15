using cCoder.Core.Models;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using Microsoft.EntityFrameworkCore;
using cCoder.Security.Data.EF.Interfaces;

namespace cCoder.Core.Services.Setup;

internal sealed class FirstTimeSetupOrchestrationService(
    IFirstTimeSetupStateService setupStateService,
    IFirstTimeSetupUserService userService,
    ICoreContextFactory coreContextFactory,
    ISecurityDbContextFactory securityDbContextFactory,
    IServiceScopeFactory serviceScopeFactory)
    : IFirstTimeSetupOrchestrationService
{
    public async Task<FirstTimeSetupResult> SetupAsync(
        FirstTimeSetupRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        if (await setupStateService.IsInitializedAsync(cancellationToken))
            throw new InvalidOperationException("The platform has already been initialised.");

        await MigrateDatabasesAsync(cancellationToken);

        if (await setupStateService.IsInitializedAsync(cancellationToken))
            throw new InvalidOperationException("The platform has already been initialised.");

        string bootstrapUserId = FirstTimeSetupIdentifiers.BuildUserId(request.Email);
        FirstTimeSetupBootstrapUser bootstrapUser = new(
            bootstrapUserId,
            request.Email.Trim(),
            request.DisplayName.Trim(),
            null);

        string tenantId = FirstTimeSetupIdentifiers.BuildTenantId(request.TenantName.Trim());

        try
        {
            using (IServiceScope tenantScope = serviceScopeFactory.CreateScope())
            {
                IFirstTimeSetupTenantService tenantService =
                    tenantScope.ServiceProvider.GetRequiredService<IFirstTimeSetupTenantService>();

                tenantId = await tenantService.SetupSecurityAsync(
                    request,
                    bootstrapUserId,
                    cancellationToken);
            }

            await userService.AuthenticateBootstrapUserAsync(
                bootstrapUser.UserId,
                request.Password,
                cancellationToken);

            using IServiceScope bootstrapScope = serviceScopeFactory.CreateScope();

            IFirstTimeSetupUserService bootstrapUserService =
                bootstrapScope.ServiceProvider.GetRequiredService<IFirstTimeSetupUserService>();
            IFirstTimeSetupAppService appService =
                bootstrapScope.ServiceProvider.GetRequiredService<IFirstTimeSetupAppService>();

            await bootstrapUserService.EnsureBootstrapCoreUserAsync(
                bootstrapUser,
                cancellationToken);

            App app = await appService.CreateFirstAppAsync(request, tenantId, cancellationToken);

            await bootstrapUserService.CompleteFirstUserRegistrationAsync(
                request,
                bootstrapUser,
                app.Id,
                cancellationToken);

            return new FirstTimeSetupResult(tenantId, app.Id, bootstrapUser.UserId);
        }
        catch (Exception ex)
        {
            await userService.RollbackAsync(
                bootstrapUser.UserId,
                cancellationToken);

            using (IServiceScope tenantScope = serviceScopeFactory.CreateScope())
            {
                IFirstTimeSetupTenantService tenantService =
                    tenantScope.ServiceProvider.GetRequiredService<IFirstTimeSetupTenantService>();

                await tenantService.RollbackAsync(
                    bootstrapUser.UserId,
                    tenantId,
                    cancellationToken);
            }

            using (IServiceScope appScope = serviceScopeFactory.CreateScope())
            {
                IFirstTimeSetupAppService appService =
                    appScope.ServiceProvider.GetRequiredService<IFirstTimeSetupAppService>();

                await appService.RollbackAsync(
                    bootstrapUser.UserId,
                    tenantId,
                    cancellationToken);
            }

            throw new InvalidOperationException(
                "First-time setup failed and the platform state was rolled back to the pre-setup state. Due to " + ex.Message,
                ex);
        }
    }

    private async Task MigrateDatabasesAsync(CancellationToken cancellationToken)
    {
        await using DbContext sso = securityDbContextFactory.CreateDbContext(true);
        await using DbContext core = coreContextFactory.CreateCoreContext();

        await sso.Database.MigrateAsync(cancellationToken);
        await core.Database.MigrateAsync(cancellationToken);
    }

    private static void ValidateRequest(FirstTimeSetupRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Domain))
            throw new InvalidOperationException("The setup request is missing the normalized domain.");
    }
}
