using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Planning;

public interface IPlanningAppService
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}
