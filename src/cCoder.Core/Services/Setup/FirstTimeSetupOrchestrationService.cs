// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        ValidateRequest(request: request);

        if (await setupStateService.IsInitializedAsync(cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("The platform has already been initialised.");
        }

        await MigrateDatabasesAsync(cancellationToken: cancellationToken);

        if (await setupStateService.IsInitializedAsync(cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("The platform has already been initialised.");
        }

        string bootstrapUserId = FirstTimeSetupIdentifiers.BuildUserId(email: request.Email);
        FirstTimeSetupBootstrapUser bootstrapUser = new(
            bootstrapUserId,
            request.Email.Trim(),
            request.DisplayName.Trim(),
            null);

        string tenantId = FirstTimeSetupIdentifiers.BuildTenantId(tenantName: request.TenantName.Trim());

        try
        {
            using (IServiceScope tenantScope = serviceScopeFactory.CreateScope())
            {
                IFirstTimeSetupTenantService tenantService =
                    tenantScope.ServiceProvider.GetRequiredService<IFirstTimeSetupTenantService>();

                tenantId = await tenantService.SetupSecurityAsync(
request: request, userId: bootstrapUserId, cancellationToken: cancellationToken);
            }

            await userService.AuthenticateBootstrapUserAsync(
userId: bootstrapUser.UserId, password: request.Password, cancellationToken: cancellationToken);

            using IServiceScope bootstrapScope = serviceScopeFactory.CreateScope();

            IFirstTimeSetupUserService bootstrapUserService =
                bootstrapScope.ServiceProvider.GetRequiredService<IFirstTimeSetupUserService>();
            IFirstTimeSetupAppService appService =
                bootstrapScope.ServiceProvider.GetRequiredService<IFirstTimeSetupAppService>();

            await bootstrapUserService.EnsureBootstrapCoreUserAsync(
bootstrapUser: bootstrapUser, cancellationToken: cancellationToken);

            App app = await appService.CreateFirstAppAsync(request: request, tenantId: tenantId, cancellationToken: cancellationToken);

            await bootstrapUserService.CompleteFirstUserRegistrationAsync(
request: request, bootstrapUser: bootstrapUser, appId: app.Id, cancellationToken: cancellationToken);

            return new FirstTimeSetupResult(tenantId, app.Id, bootstrapUser.UserId);
        }
        catch (Exception ex)
        {
            await userService.RollbackAsync(
bootstrapUserId: bootstrapUser.UserId, cancellationToken: cancellationToken);

            using (IServiceScope tenantScope = serviceScopeFactory.CreateScope())
            {
                IFirstTimeSetupTenantService tenantService =
                    tenantScope.ServiceProvider.GetRequiredService<IFirstTimeSetupTenantService>();

                await tenantService.RollbackAsync(
bootstrapUserId: bootstrapUser.UserId, tenantId: tenantId, cancellationToken: cancellationToken);
            }

            using (IServiceScope appScope = serviceScopeFactory.CreateScope())
            {
                IFirstTimeSetupAppService appService =
                    appScope.ServiceProvider.GetRequiredService<IFirstTimeSetupAppService>();

                await appService.RollbackAsync(
bootstrapUserId: bootstrapUser.UserId, tenantId: tenantId, cancellationToken: cancellationToken);
            }

            throw new InvalidOperationException(
                "First-time setup failed and the platform state was rolled back to the pre-setup state. Due to " + ex.Message,
                ex);
        }
    }

    private async Task MigrateDatabasesAsync(CancellationToken cancellationToken)
    {
        await using DbContext sso = securityDbContextFactory.CreateDbContext(ignoreAuthInfo: true);
        await using DbContext core = coreContextFactory.CreateCoreContext();

        await sso.Database.MigrateAsync(cancellationToken: cancellationToken);
        await core.Database.MigrateAsync(cancellationToken: cancellationToken);
    }

    private static void ValidateRequest(FirstTimeSetupRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(value: request.Domain))
        {
            throw new InvalidOperationException("The setup request is missing the normalized domain.");
        }
    }
}