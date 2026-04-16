using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Orchestrations;

public interface IAppOrchestrationService
{
    ValueTask<App> AddAsync(App app);
    ValueTask<App> UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}
