// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Exposures;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal sealed partial class AppSecurityAppService(
    IAppSecurityAppExposure appSecurityAppExposure)
    : IAppSecurityAppService
{
    public ValueTask AddAppAsync(App newApp) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnAdd(newApp: newApp);

            App flatApp = CreateFlatApp(
                app: newApp);

            await appSecurityAppExposure.AddAsync(
                app: flatApp);
        });

    public ValueTask UpdateAppAsync(App updatedApp) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnUpdate(updatedApp: updatedApp);

            App flatApp = CreateFlatApp(
                app: updatedApp);

            await appSecurityAppExposure.UpdateAsync(
                app: flatApp);
        });

    public ValueTask DeleteAppAsync(int appId) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnDelete(appId: appId);

            await appSecurityAppExposure.DeleteAsync(
                appId: appId);
        });

    private static App CreateFlatApp(App app) =>
        new()
        {
            Id = app.Id,
            DefaultCultureId = app.DefaultCultureId,
            TenantId = app.TenantId,
            Name = app.Name,
            Domain = app.Domain,
            DefaultTheme = app.DefaultTheme,
            ConfigJson = app.ConfigJson,
        };
}