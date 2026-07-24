// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.DocumentManagement;

internal class DocumentManagementAppService(IDocumentManagementAppBroker documentManagementAppBroker)
    : IDocumentManagementAppService
{
    public ValueTask AddAsync(App app) =>
        documentManagementAppBroker.AddAsync(app: app);
    public ValueTask UpdateAsync(App app) =>
        documentManagementAppBroker.UpdateAsync(app: app);
    public ValueTask DeleteAsync(int appId) =>
        documentManagementAppBroker.DeleteAsync(appId: appId);
}