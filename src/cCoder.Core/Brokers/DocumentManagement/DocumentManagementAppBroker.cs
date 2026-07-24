// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.DocumentManagement.Exposures;
using cCoder.DocumentManagement.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.DocumentManagement;

internal class DocumentManagementAppBroker(IDocumentManagementAppExposure documentManagementAppExposure)
    : IDocumentManagementAppBroker
{
    public ValueTask AddAsync(App app) =>
        documentManagementAppExposure.AddAsync(newApp: app);
    public ValueTask UpdateAsync(App app) =>
        documentManagementAppExposure.UpdateAsync(updatedApp: app);
    public ValueTask DeleteAsync(int appId) =>
        documentManagementAppExposure.DeleteAsync(appId: appId);
}