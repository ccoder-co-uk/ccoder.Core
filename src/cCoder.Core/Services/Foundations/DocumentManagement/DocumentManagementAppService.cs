using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.DocumentManagement;

internal class DocumentManagementAppService(IDocumentManagementAppBroker documentManagementAppBroker)
    : IDocumentManagementAppService
{
    public ValueTask AddAsync(App app) => documentManagementAppBroker.AddAsync(app);
    public ValueTask UpdateAsync(App app) => documentManagementAppBroker.UpdateAsync(app);
    public ValueTask DeleteAsync(int appId) => documentManagementAppBroker.DeleteAsync(appId);
}

