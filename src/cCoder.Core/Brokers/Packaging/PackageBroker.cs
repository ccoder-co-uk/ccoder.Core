// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Packaging;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Brokers.Packaging;

internal sealed class PackageBroker(
    IPackageManagerDependency packageManagerDependency
) : IPackageBroker
{
    public Package ExportPackage(
        int appId,
        string packageName) =>
        packageManagerDependency.ExportPackage(
            appId: appId,
            packageName: packageName);

    public ValueTask ImportPackageAsync(
        int appId,
        Package package) =>
        packageManagerDependency.ImportPackageAsync(
            appId: appId,
            package: package);
}