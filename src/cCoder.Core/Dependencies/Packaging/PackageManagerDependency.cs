// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;
using ExternalPackageManagerAggregationService =
    cCoder.Packaging.Services.Aggregations.IPackageManagerAggregationService;

namespace cCoder.Core.Dependencies.Packaging;

internal sealed class PackageManagerDependency(
    ExternalPackageManagerAggregationService packageManagerAggregationService
) : IPackageManagerDependency, ExternalPackageManagerAggregationService
{
    public Package ExportPackage(
        int appId,
        string packageName) =>
        packageManagerAggregationService.ExportPackage(
            appId: appId,
            packageName: packageName);

    public ValueTask ImportPackageAsync(
        int appId,
        Package package) =>
        packageManagerAggregationService.ImportPackageAsync(
            appId: appId,
            package: package);
}