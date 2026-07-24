// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.ContentManagement;

public interface IContentManagementAppService
{
    App GetApp(int appId, bool ignoreFilters = false);
    App GetAppByDomain(string domain, bool ignoreFilters = false);
    IQueryable<App> GetAllApps(bool ignoreFilters = false);
    ValueTask<App> AddAppAsync(App newApp);
    ValueTask<App> UpdateAppAsync(App updatedApp);
    ValueTask DeleteAsync(int appId);
}