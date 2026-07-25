// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Core.Services.Foundations.AppSecurity;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class HostedServicesAppSecurityAppAddOrchestrationService(
    IAppSecurityAppService appSecurityAppService,
    IAppSecurityUserRoleService appSecurityUserRoleService)
    : IHostedServicesAppSecurityAppAddOrchestrationService
{
    public ValueTask HandleAppAsync(App app) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnHandle(app: app);

            await appSecurityAppService.AddAppAsync(
                newApp: app);

            await appSecurityUserRoleService.SaveAppUserRolesAsync(
                app: app);
        });
}