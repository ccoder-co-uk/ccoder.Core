// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using cCoder.Workflow.Exposures;
using cCoder.Workflow.Models;


namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class SchedulingPackageProcessingService(
    IWorkflowPackageManager workflowPackageManager = null
) : ISchedulingPackageProcessingService
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            return workflowPackageManager == null
                ? ValueTask.CompletedTask
                : workflowPackageManager.ImportPackageAsync(appId: appId, package: ToExternalPackage(package: package));
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, packageName: packageName);

            return workflowPackageManager == null
                ? null
                : ToLocalPackage(package: workflowPackageManager.ExportPackage(appId: appId, packageName: packageName));
        });

    private static WorkflowPackage ToExternalPackage(Package package) =>
        package == null ? null : new WorkflowPackage
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: ToExternalPackageItem)
                .ToArray(),
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

    private static PackageItem ToLocalPackageItem(WorkflowPackageItem packageItem) =>
        packageItem == null ? null : new PackageItem
        {
            Id = packageItem.Id,
            PackageId = packageItem.PackageId,
            Type = packageItem.Type,
            Data = packageItem.Data,
        };
}