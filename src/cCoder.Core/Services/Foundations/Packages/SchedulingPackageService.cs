// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Packaging;
using cCoder.Workflow.Models;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Foundations.Packages;

internal sealed partial class SchedulingPackageService(
    ISchedulingPackageBroker schedulingPackageBroker) : ISchedulingPackageService
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            return schedulingPackageBroker.ImportPackageAsync(
                appId: appId,
                workflowPackage: ToExternalPackage(package: package));
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, packageName: packageName);

            return ToLocalPackage(package: schedulingPackageBroker.ExportPackage(
                appId: appId,
                packageName: packageName));
        });

    private static WorkflowPackage ToExternalPackage(Package package) =>
        package == null ? null : new WorkflowPackage
        {
            Id = package.Id, Name = package.Name, Description = package.Description,
            Category = package.Category, SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: item => new WorkflowPackageItem
            {
                Id = item.Id, PackageId = item.PackageId, Type = item.Type, Data = item.Data,
            })
                .ToArray(),
        };

    private static Package ToLocalPackage(WorkflowPackage package) =>
        package == null ? null : new Package
        {
            Id = package.Id, Name = package.Name, Description = package.Description,
            Category = package.Category, SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: item => new PackageItem
            {
                Id = item.Id, PackageId = item.PackageId, Type = item.Type, Data = item.Data,
            })
                .ToArray(),
        };
}