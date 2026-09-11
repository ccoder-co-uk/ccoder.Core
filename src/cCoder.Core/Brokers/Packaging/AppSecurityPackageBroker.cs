// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Exposures;
using cCoder.AppSecurity.Models;

namespace cCoder.Core.Brokers.Packaging;

internal sealed class AppSecurityPackageBroker(
    IAppSecurityPackageManager appSecurityPackageManager)
    : IAppSecurityPackageBroker
{
    public ValueTask ImportPackageAsync(int appId, AppSecurityPackage appSecurityPackage) =>
        appSecurityPackageManager.ImportPackageAsync(appId: appId, package: appSecurityPackage);

    public AppSecurityPackage ExportPackage(int appId, string packageName) =>
        appSecurityPackageManager.ExportPackage(appId: appId, packageName: packageName);
}