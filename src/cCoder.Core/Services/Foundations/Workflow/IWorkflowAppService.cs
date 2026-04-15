using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Workflow;

public interface IWorkflowAppService
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}
