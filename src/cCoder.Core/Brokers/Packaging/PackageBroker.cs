// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;
using cCoder.Packaging.Exposures;

namespace cCoder.Core.Brokers.Packaging;

internal sealed class PackageBroker(
    IPackageTransferManager packageManager
) : IPackageBroker
{
    public Package ExportPackage(
        int appId,
        string packageName) =>
        packageManager.ExportPackage(
            appId: appId,
            packageName: packageName);

    public ValueTask ImportPackageAsync(
        int appId,
        Package package) =>
        packageManager.ImportPackageAsync(
            appId: appId,
            package: package);
}