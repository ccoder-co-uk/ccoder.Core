using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.DocumentManagement;

public interface IDocumentManagementAppService
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}
