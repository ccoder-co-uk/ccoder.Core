// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Packages;
using cCoder.Data.Models.Packaging;
using PackagingManager = cCoder.Packaging.Exposures.PackageManagers.IDocumentManagementPackageManager;

namespace cCoder.Core.Exposures.PackageManagers;

internal sealed class DocumentManagementPackageManager(
    IDocumentManagementPackageProcessingService processingService)
    : PackagingManager
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        processingService.ImportPackageAsync(appId: appId, package: package);

    public Package ExportPackage(int appId, string packageName) =>
        processingService.ExportPackage(appId: appId, packageName: packageName);
}