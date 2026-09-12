// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Packaging;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Foundations.Packages;

internal sealed partial class ContentManagementPackageService(
    IContentManagementPackageBroker contentManagementPackageBroker)
    : IContentManagementPackageService
{
    public ValueTask ImportPackageAsync(int? appId, Package package) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            return contentManagementPackageBroker.ImportPackageAsync(appId: appId, package: package);
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, packageName: packageName);

            return contentManagementPackageBroker.ExportPackage(appId: appId, packageName: packageName);
        });
}