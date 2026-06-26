using cCoder.Core.Brokers.ContentManagement;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.ContentManagement;

internal class ContentManagementAppService(IContentManagementAppBroker contentManagementAppBroker)
    : IContentManagementAppService
{
    public App Get(int id, bool ignoreFilters = false) =>
        contentManagementAppBroker.Get(id, ignoreFilters);

    public App GetByDomain(string domain, bool ignoreFilters = false) =>
        contentManagementAppBroker.GetByDomain(domain, ignoreFilters);

    public IQueryable<App> GetAll(bool ignoreFilters = false) =>
        contentManagementAppBroker.GetAll(ignoreFilters);

    public async ValueTask<App> AddAsync(App app) =>
        await contentManagementAppBroker.AddAsync(app);

    public async ValueTask<App> UpdateAsync(App app) =>
        await contentManagementAppBroker.UpdateAsync(app);

    public ValueTask DeleteAsync(int appId) => contentManagementAppBroker.DeleteAsync(appId);
}

