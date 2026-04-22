using cCoder.Security.Data.Models;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Exposures;
using cCoder.Security.Objects.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Services.Setup;

internal sealed class FirstTimeSetupTenantService(
    ITenantManager tenantManager,
    ISecurityDbContextFactory securityDbContextFactory)
    : IFirstTimeSetupTenantService
{
    private const string PortalAdministratorRoleName = "Portal Administrators";

    private static readonly string[] PortalAdministratorPrivileges =
    [
        "security_admin",
        "tenant_read",
        "tenant_create",
        "tenant_update",
        "tenant_delete",
        "userrole_read",
        "userrole_create",
        "userrole_delete"
    ];

    public async Task<string> SetupSecurityAsync(
        FirstTimeSetupRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        string tenantName = request.TenantName.Trim();
        string tenantId = FirstTimeSetupIdentifiers.BuildTenantId(tenantName);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string email = request.Email.Trim();

        if (await TenantExistsAsync(tenantId, cancellationToken))
        {
            bool bootstrapUserExists = await BootstrapUserExistsAsync(userId, email, cancellationToken);

            if (!bootstrapUserExists)
            {
                throw new InvalidOperationException(
                    $"A tenant with ID '{tenantId}' already exists in SSO, but the bootstrap user '{userId}' was not found.");
            }

            await EnsureBootstrapAdministratorAccessAsync(tenantId, userId, cancellationToken);
            return tenantId;
        }

        await tenantManager.SetupAsync(
            new SetupDetails
            {
                Tenant = new Tenant
                {
                    Id = tenantId,
                    Name = tenantName,
                    Description = $"{tenantName} tenant",
                    CreatedBy = userId,
                    LastUpdatedBy = userId,
                    CreatedOn = now,
                    LastUpdated = now
                },
                User = new SSOUser
                {
                    Id = userId,
                    DisplayName = request.DisplayName.Trim(),
                    Email = request.Email.Trim(),
                    PasswordHash = request.Password
                }
            });

        await EnsureBootstrapAdministratorAccessAsync(tenantId, userId, cancellationToken);
        return tenantId;
    }

    private async Task EnsureBootstrapAdministratorAccessAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken)
    {
        await using DbContext sso = securityDbContextFactory.CreateDbContext(true);

        SSOUser user = await sso.Set<SSOUser>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(found => found.Id == userId, cancellationToken);

        if (user is null)
            throw new InvalidOperationException($"Bootstrap SSO user '{userId}' was not found after tenant setup.");

        SSORole tenantAdministratorRole = await sso.Set<SSORole>()
            .IgnoreQueryFilters()
            .Where(role => role.TenantId == tenantId && role.UsersArePortalAdmins)
            .OrderBy(role => role.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenantAdministratorRole is not null)
        {
            tenantAdministratorRole.Privs = EnsurePrivileges(
                tenantAdministratorRole.Privs,
                "tenant_read");

            await EnsureUserRoleAsync(sso, user.Id, tenantAdministratorRole.Id, cancellationToken);
        }

        SSORole portalAdministratorRole = await sso.Set<SSORole>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                role => role.TenantId == null
                    && role.UsersArePortalAdmins
                    && role.Name == PortalAdministratorRoleName,
                cancellationToken);

        if (portalAdministratorRole is null)
        {
            portalAdministratorRole = new SSORole
            {
                Id = Guid.NewGuid(),
                Name = PortalAdministratorRoleName,
                Description = "Bootstrap portal administrators",
                UsersArePortalAdmins = true,
                Privs = string.Join(',', PortalAdministratorPrivileges),
                TenantId = null
            };

            await sso.Set<SSORole>().AddAsync(portalAdministratorRole, cancellationToken);
        }
        else
        {
            portalAdministratorRole.Privs = EnsurePrivileges(
                portalAdministratorRole.Privs,
                PortalAdministratorPrivileges);
        }

        await EnsureUserRoleAsync(sso, user.Id, portalAdministratorRole.Id, cancellationToken);
        await sso.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureUserRoleAsync(
        DbContext sso,
        string userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        bool exists = await sso.Set<SSOUserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(
                userRole => userRole.UserId == userId && userRole.RoleId == roleId,
                cancellationToken);

        if (exists)
            return;

        await sso.Set<SSOUserRole>().AddAsync(
            new SSOUserRole
            {
                UserId = userId,
                RoleId = roleId
            },
            cancellationToken);
    }

    private static string EnsurePrivileges(string existingPrivileges, params string[] requiredPrivileges)
    {
        HashSet<string> privileges = new(
            (existingPrivileges ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            StringComparer.Ordinal);

        foreach (string privilege in requiredPrivileges)
            privileges.Add(privilege);

        return string.Join(',', privileges.OrderBy(privilege => privilege, StringComparer.Ordinal));
    }

    private async Task<bool> TenantExistsAsync(string tenantId, CancellationToken cancellationToken)
    {
        await using DbContext sso = securityDbContextFactory.CreateDbContext(true);

        return await sso.Set<Tenant>()
            .IgnoreQueryFilters()
            .AnyAsync(found => found.Id == tenantId, cancellationToken);
    }

    private async Task<bool> BootstrapUserExistsAsync(string userId, string email, CancellationToken cancellationToken)
    {
        await using DbContext sso = securityDbContextFactory.CreateDbContext(true);

        return await sso.Set<SSOUser>()
            .IgnoreQueryFilters()
            .AnyAsync(
                found => found.Id == userId || found.Email == email,
                cancellationToken);
    }
}

