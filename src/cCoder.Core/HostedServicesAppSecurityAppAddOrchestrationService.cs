using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using AppSecurityAppOrchestrationService = cCoder.AppSecurity.Services.Orchestrations.IAppOrchestrationService;
using AppSecurityUserRoleBroker = cCoder.AppSecurity.Brokers.Storages.IUserRoleBroker;

namespace cCoder.Core;

internal sealed class HostedServicesAppSecurityAppAddOrchestrationService(
    AppSecurityAppOrchestrationService appOrchestrationService,
    AppSecurityUserRoleBroker userRoleBroker)
{
    public async ValueTask HandleAsync(App app)
    {
        ArgumentNullException.ThrowIfNull(app);

        await appOrchestrationService.AddAsync(app);
        await SaveRoleUsersAsync(app);
    }

    private async ValueTask SaveRoleUsersAsync(App app)
    {
        UserRole[] userRoles =
            [.. (app.Roles ?? [])
                .SelectMany(role => role.Users ?? [])
                .Where(userRole =>
                    userRole is not null &&
                    userRole.RoleId != Guid.Empty &&
                    !string.IsNullOrWhiteSpace(userRole.UserId))
                .GroupBy(userRole => $"{userRole.RoleId:N}:{userRole.UserId}", StringComparer.OrdinalIgnoreCase)
                .Select(group => new UserRole
                {
                    RoleId = group.First().RoleId,
                    UserId = group.First().UserId
                })];

        foreach (UserRole userRole in userRoles)
        {
            bool exists = userRoleBroker
                .GetAllUserRoles(ignoreFilters: true)
                .Any(existing =>
                    existing.RoleId == userRole.RoleId &&
                    existing.UserId == userRole.UserId);

            if (!exists)
                await userRoleBroker.AddUserRoleAsync(userRole);
        }
    }
}
