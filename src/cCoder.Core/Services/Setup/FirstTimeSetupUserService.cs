// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models.Security;
using cCoder.Security.Services.Aggregations.Interfaces;
using Microsoft.EntityFrameworkCore;
using cCoder.Core.Models;
using System.CodeDom.Compiler;

namespace cCoder.Core.Services.Setup;

[GeneratedCode("decompilation-recovery", "1.0")]
internal sealed class FirstTimeSetupUserService(
    IAuthenticationAggregationService authenticationAggregationService,
    ICoreContextFactory coreContextFactory)
    : IFirstTimeSetupUserService
{
    public async Task AuthenticateBootstrapUserAsync(
        string userId,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(value: userId))
        {
            throw new InvalidOperationException("Bootstrap user ID is required.");
        }

        await authenticationAggregationService.LoginAsync(
            username: userId,
            password: password);
    }

    public async Task EnsureBootstrapCoreUserAsync(
        FirstTimeSetupBootstrapUser bootstrapUser,
        CancellationToken cancellationToken = default)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        bool exists = await core.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: user => user.Id == bootstrapUser.UserId, cancellationToken: cancellationToken);

        if (exists)
        {
            return;
        }

        await core.Set<User>()
            .AddAsync(
entity: new User
{
    Id = bootstrapUser.UserId,
    Email = bootstrapUser.Email,
    DisplayName = bootstrapUser.DisplayName,
    DefaultCultureId = string.Empty,
    IsActive = true
}, cancellationToken: cancellationToken);

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    public async Task CompleteFirstUserRegistrationAsync(
        FirstTimeSetupRequest request,
        FirstTimeSetupBootstrapUser bootstrapUser,
        int appId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureRequiredRoleMembershipsAsync(appId: appId, userId: bootstrapUser.UserId, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed while attaching the first admin role membership.", ex);
        }
    }

    public async Task RollbackAsync(
        string bootstrapUserId,
        CancellationToken cancellationToken = default)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        User coreUser = await core.Set<User>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.Id == bootstrapUserId, cancellationToken: cancellationToken);

        if (coreUser is null)
        {
            return;
        }

        UserRole[] userRoles = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.UserId == bootstrapUserId)
            .ToArrayAsync(cancellationToken: cancellationToken);

        core.RemoveRange(entities: userRoles);
        core.Remove(entity: coreUser);
        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private async Task EnsureRequiredRoleMembershipsAsync(
        int appId,
        string userId,
        CancellationToken cancellationToken)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        Role[] roles = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(predicate: role =>
                role.AppId == appId
                && (role.Name == "Administrators" || role.Name == "Users"))
            .ToArrayAsync(cancellationToken: cancellationToken);

        foreach (Role role in roles)
        {
            bool exists = await core.Set<UserRole>()
                .IgnoreQueryFilters()
                .AnyAsync(
predicate: userRole => userRole.RoleId == role.Id && userRole.UserId == userId, cancellationToken: cancellationToken);

            if (exists)
            {
                continue;
            }

            await core.Set<UserRole>()
                .AddAsync(
entity: new UserRole
{
    RoleId = role.Id,
    UserId = userId
}, cancellationToken: cancellationToken);
        }

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}