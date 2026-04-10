using cCoder.AppSecurity.Exposures;
using cCoder.AppSecurity.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using PackagingBroker = cCoder.Packaging.Brokers.IAppSecurityPackageManagerBroker;


namespace cCoder.Core.Brokers.Packaging;

internal class AppSecurityPackageManagerBroker(
    IAppSecurityPackageManager appSecurityPackageManager = null
) : PackagingBroker
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        appSecurityPackageManager == null
            ? ValueTask.CompletedTask
            : appSecurityPackageManager.ImportPackageAsync(appId, ToExternalPackage(package));

    public Package ExportPackage(int appId, string packageName) =>
        appSecurityPackageManager == null
            ? null
            : ToLocalPackage(appSecurityPackageManager.ExportPackage(appId, packageName));

    private static AppSecurityPackage ToExternalPackage(Package package) =>
        package == null ? null : new AppSecurityPackage(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToExternalPackageItem).ToArray(),
        };

    private static AppSecurityPackageItem ToExternalPackageItem(PackageItem packageItem) =>
        packageItem == null ? null : new AppSecurityPackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };

    private static Package ToLocalPackage(AppSecurityPackage package) =>
        package == null ? null : new Package(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToLocalPackageItem).ToArray(),
        };

    private static PackageItem ToLocalPackageItem(AppSecurityPackageItem packageItem) =>
        packageItem == null ? null : new PackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };
}


