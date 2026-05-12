using cCoder.Data;
using cCoder.Data.Models.Security;
using cCoder.Security.Exposures;
using Microsoft.EntityFrameworkCore;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Setup;

internal sealed class FirstTimeSetupUserService(
    IAccountManager accountManager,
    ICoreContextFactory coreContextFactory)
    : IFirstTimeSetupUserService
{
    public async Task AuthenticateBootstrapUserAsync(
        string userId,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Bootstrap user ID is required.");

        await accountManager.LoginAsync(userId, password);
    }

    public async Task EnsureBootstrapCoreUserAsync(
        FirstTimeSetupBootstrapUser bootstrapUser,
        CancellationToken cancellationToken = default)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        bool exists = await core.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(user => user.Id == bootstrapUser.UserId, cancellationToken);

        if (exists)
            return;

        await core.Set<User>().AddAsync(
            new User
            {
                Id = bootstrapUser.UserId,
                Email = bootstrapUser.Email,
                DisplayName = bootstrapUser.DisplayName,
                DefaultCultureId = string.Empty,
                IsActive = true
            },
            cancellationToken);

        await core.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteFirstUserRegistrationAsync(
        FirstTimeSetupRequest request,
        FirstTimeSetupBootstrapUser bootstrapUser,
        int appId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureRequiredRoleMembershipsAsync(appId, bootstrapUser.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed while attaching the first admin role membership.", ex);
        }
    }

    private async Task EnsureRequiredRoleMembershipsAsync(
        int appId,
        string userId,
        CancellationToken cancellationToken)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        Role[] roles = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(role =>
                role.AppId == appId
                && (role.Name == "Administrators" || role.Name == "Users"))
            .ToArrayAsync(cancellationToken);

        foreach (Role role in roles)
        {
            bool exists = await core.Set<UserRole>()
                .IgnoreQueryFilters()
                .AnyAsync(
                    userRole => userRole.RoleId == role.Id && userRole.UserId == userId,
                    cancellationToken);

            if (exists)
                continue;

            await core.Set<UserRole>().AddAsync(
                new UserRole
                {
                    RoleId = role.Id,
                    UserId = userId
                },
                cancellationToken);
        }

        await core.SaveChangesAsync(cancellationToken);
    }
}

