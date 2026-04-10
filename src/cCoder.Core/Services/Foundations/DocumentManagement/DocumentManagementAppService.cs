using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.DocumentManagement;

internal class DocumentManagementAppService(IDocumentManagementAppBroker documentManagementAppBroker)
    : IDocumentManagementAppService
{
    public ValueTask AddAsync(App app) => documentManagementAppBroker.AddAsync(ToLocalApp(app));
    public ValueTask UpdateAsync(App app) => documentManagementAppBroker.UpdateAsync(ToLocalApp(app));
    public ValueTask DeleteAsync(int appId) => documentManagementAppBroker.DeleteAsync(appId);

    private static cCoder.Data.Models.CMS.App ToLocalApp(App app) =>
        app == null
            ? null
            : new cCoder.Data.Models.CMS.App
            {
                Id = app.Id,
                Name = app.Name,
                Folders = app.Folders?.ToArray(),
            };
}

