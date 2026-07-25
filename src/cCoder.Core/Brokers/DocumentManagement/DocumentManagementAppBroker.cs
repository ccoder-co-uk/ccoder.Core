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
    public ValueTask AddAppAsync(App newApp) =>
        documentManagementAppExposure.AddAsync(newApp: newApp);

    public ValueTask UpdateAppAsync(App updatedApp) =>
        documentManagementAppExposure.UpdateAsync(updatedApp: updatedApp);

    public ValueTask DeleteAppAsync(int appId) =>
        documentManagementAppExposure.DeleteAsync(appId: appId);
}