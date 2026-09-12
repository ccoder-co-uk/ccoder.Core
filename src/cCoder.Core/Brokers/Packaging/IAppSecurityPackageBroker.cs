// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Models;

namespace cCoder.Core.Brokers.Packaging;

internal interface IAppSecurityPackageBroker
{
    ValueTask ImportPackageAsync(int appId, AppSecurityPackage appSecurityPackage);
    AppSecurityPackage ExportPackage(int appId, string packageName);
}