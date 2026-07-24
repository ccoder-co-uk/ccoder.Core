// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using AppSecurityAppOrchestrationService = cCoder.AppSecurity.Services.Orchestrations.IAppOrchestrationService;
using AppSecurityUserRoleBroker = cCoder.AppSecurity.Brokers.Storages.IUserRoleBroker;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class HostedServicesAppSecurityAppAddOrchestrationService(
    AppSecurityAppOrchestrationService appOrchestrationService,
    AppSecurityUserRoleBroker userRoleBroker)
    : IHostedServicesAppSecurityAppAddOrchestrationService
{
    public ValueTask HandleAppAsync(App app) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnHandle(app: app);

            await appOrchestrationService.AddAppAsync(app: app);
            await SaveRoleUsersAsync(app: app);
        });

    private async ValueTask SaveRoleUsersAsync(App app)
    {
        UserRole[] userRoles =
            [.. (app.Roles ?? [])
                .SelectMany(selector: role => role.Users ?? [])
                .Where(predicate: userRole =>
                    userRole is not null &&
                    userRole.RoleId != Guid.Empty &&
                    !string.IsNullOrWhiteSpace(value: userRole.UserId))
                .GroupBy(keySelector: userRole => $"{userRole.RoleId:N}:{userRole.UserId}",comparer: StringComparer.OrdinalIgnoreCase)
                .Select(selector: group => new UserRole
                {
                    RoleId = group.First().RoleId,
                    UserId = group.First().UserId
                })];

        foreach (UserRole userRole in userRoles)
        {
            bool exists = userRoleBroker
                .GetAllUserRoles(ignoreFilters: true)
                .Any(predicate: existing =>
                    existing.RoleId == userRole.RoleId &&
                    existing.UserId == userRole.UserId);

            if (!exists)
            {
                await userRoleBroker.AddUserRoleAsync(entity: userRole);
            }
        }
    }
}