using cCoder.Core.Models;

namespace cCoder.Core.Services.Orchestrations;

public interface IAppOrchestrationService
{
    ValueTask<App> AddAsync(App app);
    ValueTask<App> UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}
