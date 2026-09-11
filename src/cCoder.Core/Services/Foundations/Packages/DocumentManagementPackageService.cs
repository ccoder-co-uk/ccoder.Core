// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Packaging;
using cCoder.DocumentManagement.Models;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Foundations.Packages;

internal sealed partial class DocumentManagementPackageService(
    IDocumentManagementPackageBroker documentManagementPackageBroker)
    : IDocumentManagementPackageService
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            return documentManagementPackageBroker.ImportPackageAsync(
                appId: appId,
                documentManagementPackage: ToExternalPackage(package: package));
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, packageName: packageName);

            return ToLocalPackage(package: documentManagementPackageBroker.ExportPackage(
                appId: appId,
                packageName: packageName));
        });

    private static DocumentManagementPackage ToExternalPackage(Package package) =>
        package == null ? null : new DocumentManagementPackage
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: item => new DocumentManagementPackageItem
            {
                Id = item.Id,
                PackageId = item.PackageId,
                Type = item.Type,
                Data = item.Data,
            })
                .ToArray(),
        };

    private static Package ToLocalPackage(DocumentManagementPackage package) =>
        package == null ? null : new Package
        {
            Name = package.Name,
            Id = package.Id,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: item => new PackageItem
            {
                Id = item.Id,
                PackageId = item.PackageId,
                Type = item.Type,
                Data = item.Data,
            })
                .ToArray(),
        };
}