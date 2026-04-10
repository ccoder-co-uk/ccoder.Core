using App = cCoder.Data.Models.CMS.App;

namespace cCoder.Core.Brokers.ContentManagement;

public interface IContentManagementAppBroker
{
    App Get(int id, bool ignoreFilters = false);
    App GetByDomain(string domain, bool ignoreFilters = false);
    ValueTask<App> AddAsync(App app);
    ValueTask<App> UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}

