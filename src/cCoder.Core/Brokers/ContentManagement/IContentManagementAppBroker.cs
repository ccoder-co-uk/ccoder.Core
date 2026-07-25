// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using App = cCoder.Data.Models.CMS.App;

namespace cCoder.Core.Brokers.ContentManagement;

public interface IContentManagementAppBroker
{
    App GetApp(int appId, bool ignoreFilters = false);
    App GetAppByDomain(string domain, bool ignoreFilters = false);
    IQueryable<App> GetAllApps(bool ignoreFilters = false);
    ValueTask<App> AddAppAsync(App newApp);
    ValueTask<App> UpdateAppAsync(App updatedApp);
    ValueTask DeleteAppAsync(int appId);
}