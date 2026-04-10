using cCoder.ContentManagement.Exposures;
using App = cCoder.Data.Models.CMS.App;

namespace cCoder.Core.Brokers.ContentManagement;

internal class ContentManagementAppBroker(IContentManagementAppExposure contentManagementAppExposure)
    : IContentManagementAppBroker
{
    public App Get(int id, bool ignoreFilters = false) =>
        contentManagementAppExposure.Get(id, ignoreFilters);

    public App GetByDomain(string domain, bool ignoreFilters = false) =>
        contentManagementAppExposure.GetByDomain(domain, ignoreFilters);

    public ValueTask<App> AddAsync(App app) => contentManagementAppExposure.AddAsync(app);
    public ValueTask<App> UpdateAsync(App app) => contentManagementAppExposure.UpdateAsync(app);
    public ValueTask DeleteAsync(int appId) => contentManagementAppExposure.DeleteAsync(appId);
}

