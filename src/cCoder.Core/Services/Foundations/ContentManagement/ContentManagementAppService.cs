// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.ContentManagement;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.ContentManagement;

internal sealed partial class ContentManagementAppService(
    IContentManagementAppBroker contentManagementAppBroker)
    : IContentManagementAppService
{
    public App GetApp(int appId, bool ignoreFilters = false) =>
        TryCatch(operation: () =>
        {
            ValidateAppOnGet(appId: appId, ignoreFilters: ignoreFilters);

            return contentManagementAppBroker.GetApp(
                appId: appId,
                ignoreFilters: ignoreFilters);
        });

    public App GetAppByDomain(string domain, bool ignoreFilters = false) =>
        TryCatch(operation: () =>
        {
            ValidateAppByDomainOnGet(
                domain: domain,
                ignoreFilters: ignoreFilters);

            return contentManagementAppBroker.GetAppByDomain(
                domain: domain,
                ignoreFilters: ignoreFilters);
        });

    public IQueryable<App> GetAllApps(bool ignoreFilters = false) =>
        TryCatch(operation: () =>
        {
            ValidateAppsOnGet(ignoreFilters: ignoreFilters);

            return contentManagementAppBroker.GetAllApps(
                ignoreFilters: ignoreFilters);
        });

    public ValueTask<App> AddAppAsync(App newApp) =>
        TryCatch(operation: async ValueTask<App> () =>
        {
            ValidateAppOnAdd(newApp: newApp);

            App flatApp = CreateFlatApp(app: newApp);

            App persistedApp = await contentManagementAppBroker.AddAppAsync(
                newApp: flatApp);

            ApplyPersistedValues(targetApp: newApp, persistedApp: persistedApp);

            return newApp;
        });

    public ValueTask<App> UpdateAppAsync(App updatedApp) =>
        TryCatch(operation: async ValueTask<App> () =>
        {
            ValidateAppOnUpdate(updatedApp: updatedApp);

            App flatApp = CreateFlatApp(app: updatedApp);

            App persistedApp = await contentManagementAppBroker.UpdateAppAsync(
                updatedApp: flatApp);

            ApplyPersistedValues(
                targetApp: updatedApp,
                persistedApp: persistedApp);

            return updatedApp;
        });

    public ValueTask DeleteAsync(int appId) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnDelete(appId: appId);

            await contentManagementAppBroker.DeleteAsync(appId: appId);
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

    private static void ApplyPersistedValues(App targetApp, App persistedApp)
    {
        targetApp.Id = persistedApp.Id;
        targetApp.DefaultCultureId = persistedApp.DefaultCultureId;
        targetApp.TenantId = persistedApp.TenantId;
        targetApp.Name = persistedApp.Name;
        targetApp.Domain = persistedApp.Domain;
        targetApp.DefaultTheme = persistedApp.DefaultTheme;
        targetApp.ConfigJson = persistedApp.ConfigJson;
    }
}