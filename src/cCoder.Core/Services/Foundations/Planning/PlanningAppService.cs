// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Planning;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Planning;

internal sealed partial class PlanningAppService(IPlanningAppBroker planningAppBroker)
    : IPlanningAppService
{
    public ValueTask AddAppAsync(App newApp) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnAdd(newApp: newApp);

            App flatApp = CreateFlatApp(app: newApp);

            await planningAppBroker.AddAppAsync(newApp: flatApp);
        });

    public ValueTask UpdateAppAsync(App updatedApp) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnUpdate(updatedApp: updatedApp);

            App flatApp = CreateFlatApp(app: updatedApp);

            await planningAppBroker.UpdateAppAsync(updatedApp: flatApp);
        });

    public ValueTask DeleteAppAsync(int appId) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnDelete(appId: appId);

            await planningAppBroker.DeleteAppAsync(appId: appId);
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