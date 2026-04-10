using cCoder.ContentManagement.Exposures;
using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using PackagingBroker = cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker;
using DataPackage = cCoder.Data.Models.Packaging.Package;
using DataPackageItem = cCoder.Data.Models.Packaging.PackageItem;


namespace cCoder.Core.Brokers.Packaging;

internal class ContentManagementPackageManagerBroker(
    IContentManagementPackageManager contentManagementPackageManager
) : PackagingBroker
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        contentManagementPackageManager.ImportPackageAsync(appId, ToExternalPackage(package));

    public Package ExportPackage(int appId, string packageName) =>
        ToLocalPackage(contentManagementPackageManager.ExportPackage(appId, packageName));

    private static DataPackage ToExternalPackage(Package package) =>
        package == null ? null : new DataPackage(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToExternalPackageItem).ToArray(),
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
        package == null ? null : new Package(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToLocalPackageItem).ToArray(),
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


