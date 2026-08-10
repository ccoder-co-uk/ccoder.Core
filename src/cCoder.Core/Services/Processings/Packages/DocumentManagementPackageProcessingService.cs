// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.DocumentManagement.Exposures;
using cCoder.DocumentManagement.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Security;
using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;


namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class DocumentManagementPackageProcessingService(
    IDocumentManagementPackageManager documentManagementPackageManager = null
) : IDocumentManagementPackageProcessingService
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            return documentManagementPackageManager == null
                ? ValueTask.CompletedTask
                : documentManagementPackageManager.ImportPackageAsync(appId: appId, package: ToExternalPackage(package: package));
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, packageName: packageName);

            return documentManagementPackageManager == null
                ? null
                : ToLocalPackage(package: documentManagementPackageManager.ExportPackage(appId: appId, packageName: packageName));
        });

    private static DocumentManagementPackage ToExternalPackage(Package package) =>
        package == null ? null : new DocumentManagementPackage
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: ToExternalPackageItem)
                .ToArray(),
        };

    private static DocumentManagementPackageItem ToExternalPackageItem(PackageItem packageItem) =>
        packageItem == null ? null : new DocumentManagementPackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };

    private static Package ToLocalPackage(DocumentManagementPackage package) =>
        package == null ? null : new Package
        {
            Name = package.Name,
            Id = package.Id,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: ToLocalPackageItem)
                .ToArray(),
        };

    private static PackageItem ToLocalPackageItem(DocumentManagementPackageItem packageItem) =>
        packageItem == null ? null : new PackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };
}