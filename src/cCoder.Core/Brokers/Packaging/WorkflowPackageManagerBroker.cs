using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using cCoder.Workflow.Exposures;
using cCoder.Workflow.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;
using PackagingBroker = cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker;


namespace cCoder.Core.Brokers.Packaging;

internal class WorkflowPackageManagerBroker(
    IWorkflowPackageManager workflowPackageManager = null
) : PackagingBroker
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        workflowPackageManager == null
            ? ValueTask.CompletedTask
            : workflowPackageManager.ImportPackageAsync(appId, ToExternalPackage(package));

    public Package ExportPackage(int appId, string packageName) =>
        workflowPackageManager == null
            ? null
            : ToLocalPackage(workflowPackageManager.ExportPackage(appId, packageName));

    private static WorkflowPackage ToExternalPackage(Package package) =>
        package == null ? null : new WorkflowPackage(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToExternalPackageItem).ToArray(),
        };

    private static WorkflowPackageItem ToExternalPackageItem(PackageItem packageItem) =>
        packageItem == null ? null : new WorkflowPackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };

    private static Package ToLocalPackage(WorkflowPackage package) =>
        package == null ? null : new Package(package.Name)
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(ToLocalPackageItem).ToArray(),
        };

    private static PackageItem ToLocalPackageItem(WorkflowPackageItem packageItem) =>
        packageItem == null ? null : new PackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };
}


