using cCoder.Data;
using cCoder.Security.Data.EF.Interfaces;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Services.Setup;

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

        string tenantId;
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

        var app = await appService.CreateFirstAppAsync(request, tenantId, cancellationToken);

        await bootstrapUserService.CompleteFirstUserRegistrationAsync(
            request,
            bootstrapUser,
            app.Id,
            cancellationToken);

        return new FirstTimeSetupResult(tenantId, app.Id, bootstrapUser.UserId);
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
