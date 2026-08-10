// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures;
using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using DataPackage = cCoder.Data.Models.Packaging.Package;
using DataPackageItem = cCoder.Data.Models.Packaging.PackageItem;


namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class ContentManagementPackageProcessingService(
    IContentManagementPackageManager contentManagementPackageManager
) : IContentManagementPackageProcessingService
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            return contentManagementPackageManager.ImportPackageAsync(appId: appId, package: ToExternalPackage(package: package));
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, packageName: packageName);

            return ToLocalPackage(package: contentManagementPackageManager.ExportPackage(appId: appId, packageName: packageName));
        });

    private static DataPackage ToExternalPackage(Package package) =>
        package == null ? null : new DataPackage()
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: ToExternalPackageItem)
                .ToArray(),
        };

    private static DataPackageItem ToExternalPackageItem(PackageItem packageItem) =>
        packageItem == null ? null : new DataPackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };

    private static Package ToLocalPackage(DataPackage package) =>
        package == null ? null : new Package()
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: ToLocalPackageItem)
                .ToArray(),
        };

    private static PackageItem ToLocalPackageItem(DataPackageItem packageItem) =>
        packageItem == null ? null : new PackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };
}