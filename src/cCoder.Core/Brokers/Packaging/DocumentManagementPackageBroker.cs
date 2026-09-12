// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.DocumentManagement.Exposures;
using cCoder.DocumentManagement.Models;

namespace cCoder.Core.Brokers.Packaging;

internal sealed class DocumentManagementPackageBroker(
    IDocumentManagementPackageManager documentManagementPackageManager)
    : IDocumentManagementPackageBroker
{
    public ValueTask ImportPackageAsync(int appId, DocumentManagementPackage documentManagementPackage) =>
        documentManagementPackageManager.ImportPackageAsync(
            appId: appId,
            documentManagementPackage: documentManagementPackage);

    public DocumentManagementPackage ExportPackage(int appId, string packageName) =>
        documentManagementPackageManager.ExportPackage(appId: appId, packageName: packageName);
}