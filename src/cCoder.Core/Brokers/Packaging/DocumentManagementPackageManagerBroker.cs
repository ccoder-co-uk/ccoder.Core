using cCoder.DocumentManagement.Exposures;
using cCoder.DocumentManagement.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Security;
using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using PackagingBroker = cCoder.Packaging.Brokers.IDocumentManagementPackageManagerBroker;


namespace cCoder.Core.Brokers.Packaging;

internal class DocumentManagementPackageManagerBroker(
    IDocumentManagementPackageManager documentManagementPackageManager = null
) : PackagingBroker
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        documentManagementPackageManager == null
            ? ValueTask.CompletedTask
            : documentManagementPackageManager.ImportPackageAsync(appId, ToExternalPackage(package));

    public Package ExportPackage(int appId, string packageName) =>
        documentManagementPackageManager == null
            ? null
            : ToLocalPackage(documentManagementPackageManager.ExportPackage(appId, packageName));

    private static DocumentManagementPackage ToExternalPackage(Package package) =>
        package == null ? null : new DocumentManagementPackage(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToExternalPackageItem).ToArray(),
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
        package == null ? null : new Package(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToLocalPackageItem).ToArray(),
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


