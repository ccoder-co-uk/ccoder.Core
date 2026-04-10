using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using cCoder.Scheduling.Exposures;
using cCoder.Scheduling.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;
using PackagingBroker = cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker;


namespace cCoder.Core.Brokers.Packaging;

internal class SchedulingPackageManagerBroker(
    ISchedulingPackageManager schedulingPackageManager = null
) : PackagingBroker
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        schedulingPackageManager == null
            ? ValueTask.CompletedTask
            : schedulingPackageManager.ImportPackageAsync(appId, ToExternalPackage(package));

    public Package ExportPackage(int appId, string packageName) =>
        schedulingPackageManager == null
            ? null
            : ToLocalPackage(schedulingPackageManager.ExportPackage(appId, packageName));

    private static SchedulingPackage ToExternalPackage(Package package) =>
        package == null ? null : new SchedulingPackage(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToExternalPackageItem).ToArray(),
        };

    private static SchedulingPackageItem ToExternalPackageItem(PackageItem packageItem) =>
        packageItem == null ? null : new SchedulingPackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };

    private static Package ToLocalPackage(SchedulingPackage package) =>
        package == null ? null : new Package(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToLocalPackageItem).ToArray(),
        };

    private static PackageItem ToLocalPackageItem(SchedulingPackageItem packageItem) =>
        packageItem == null ? null : new PackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };
}


