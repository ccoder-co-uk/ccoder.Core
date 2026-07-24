// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using App = cCoder.Data.Models.CMS.App;
using cCoder.ContentManagement.Exposures;

namespace cCoder.Core.Brokers.ContentManagement;

internal class ContentManagementAppBroker(IAppManager appManager)
    : IContentManagementAppBroker
{
    public App Get(int id, bool ignoreFilters = false) =>
        appManager.Get(appManagerId: id, ignoreFilters: ignoreFilters);

    public App GetByDomain(string domain, bool ignoreFilters = false) =>
        appManager.GetByDomain(domain: domain, ignoreFilters: ignoreFilters);

    public IQueryable<App> GetAll(bool ignoreFilters = false) =>
        appManager.GetAll(ignoreFilters: ignoreFilters);

    public ValueTask<App> AddAsync(App app) =>
        appManager.AddAsync(newApp: app);
    public ValueTask<App> UpdateAsync(App app) =>
        appManager.UpdateAsync(updatedApp: app);
    public ValueTask DeleteAsync(int appId) =>
        appManager.DeleteAsync(appId: appId);
}