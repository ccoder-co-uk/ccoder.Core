using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.ContentManagement;

public interface IContentManagementAppService
{
    App Get(int id, bool ignoreFilters = false);
    App GetByDomain(string domain, bool ignoreFilters = false);
    IQueryable<App> GetAll(bool ignoreFilters = false);
    ValueTask<App> AddAsync(App app);
    ValueTask<App> UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}
