using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.Planning;

public interface IPlanningAppService
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}
