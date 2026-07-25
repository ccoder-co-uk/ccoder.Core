// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Brokers.Storages;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal sealed partial class AppSecurityUserRoleService(
    IUserRoleBroker userRoleBroker)
    : IAppSecurityUserRoleService
{
    public ValueTask SaveAppUserRolesAsync(App app) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnSave(app: app);

            UserRole[] userRoles =
                [.. (app.Roles ?? [])
                    .SelectMany(selector: role =>
                        role.Users ?? [])
                    .Where(predicate: userRole =>
                        userRole is not null
                        && userRole.RoleId != Guid.Empty
                        && !string.IsNullOrWhiteSpace(
                            value: userRole.UserId))
                    .GroupBy(
                        keySelector: userRole =>
                            $"{userRole.RoleId:N}:{userRole.UserId}",
                        comparer:
                            StringComparer.OrdinalIgnoreCase)
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
                        existing.RoleId == userRole.RoleId
                        && existing.UserId == userRole.UserId);

                if (!exists)
                {
                    await userRoleBroker.AddUserRoleAsync(
                        entity: userRole);
                }
            }
        });
}