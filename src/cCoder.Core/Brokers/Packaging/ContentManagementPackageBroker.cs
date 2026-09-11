// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Brokers.Packaging;

internal sealed class ContentManagementPackageBroker(
    IContentManagementPackageManager contentManagementPackageManager)
    : IContentManagementPackageBroker
{
    public ValueTask ImportPackageAsync(int? appId, Package package) =>
        contentManagementPackageManager.ImportPackageAsync(appId: appId, package: package);

    public Package ExportPackage(int appId, string packageName) =>
        contentManagementPackageManager.ExportPackage(appId: appId, packageName: packageName);
}