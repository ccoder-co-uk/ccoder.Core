// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using App = cCoder.Data.Models.CMS.App;
using cCoder.ContentManagement.Exposures;

namespace cCoder.Core.Brokers.ContentManagement;

internal class ContentManagementAppBroker(IAppManager appManager)
    : IContentManagementAppBroker
{
    public App GetApp(int appId, bool ignoreFilters = false) =>
        appManager.Get(appManagerId: appId, ignoreFilters: ignoreFilters);

    public App GetAppByDomain(string domain, bool ignoreFilters = false) =>
        appManager.GetByDomain(domain: domain, ignoreFilters: ignoreFilters);

    public IQueryable<App> GetAllApps(bool ignoreFilters = false) =>
        appManager.GetAll(ignoreFilters: ignoreFilters);

    public ValueTask<App> AddAppAsync(App newApp) =>
        appManager.AddAsync(newApp: newApp);

    public ValueTask<App> UpdateAppAsync(App updatedApp) =>
        appManager.UpdateAsync(updatedApp: updatedApp);

    public ValueTask DeleteAppAsync(int appId) =>
        appManager.DeleteAsync(appId: appId);
}