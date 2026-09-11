// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Packaging;
using cCoder.AppSecurity.Models;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Foundations.Packages;

internal sealed partial class AppSecurityPackageService(
    IAppSecurityPackageBroker appSecurityPackageBroker) : IAppSecurityPackageService
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            return appSecurityPackageBroker.ImportPackageAsync(
                appId: appId,
                appSecurityPackage: ToExternalPackage(package: package));
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, packageName: packageName);

            return ToLocalPackage(package: appSecurityPackageBroker.ExportPackage(
                appId: appId,
                packageName: packageName));
        });

    private static AppSecurityPackage ToExternalPackage(Package package) =>
        package == null ? null : new AppSecurityPackage
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = package.Items?.Select(selector: item => new AppSecurityPackageItem
            {
                Id = item.Id,
                PackageId = item.PackageId,
                Type = item.Type,
                Data = item.Data,
            })
                .ToArray(),
        };

    private static Package ToLocalPackage(AppSecurityPackage package) =>
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