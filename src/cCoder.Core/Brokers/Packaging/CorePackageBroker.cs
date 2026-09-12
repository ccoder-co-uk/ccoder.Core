// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Packages;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Brokers.Packaging;

internal sealed class CorePackageBroker(
    CorePackageDependency corePackageDependency) : ICorePackageBroker
{
    public ValueTask ImportAppConfigurationAsync(int appId, string data) =>
        corePackageDependency.ImportAppConfigurationAsync(
            appId: appId,
            data: data);

    public ValueTask<Package> ExportAppConfigurationAsync(int appId, string sourceApi) =>
        corePackageDependency.ExportAppConfigurationAsync(
            appId: appId,
            sourceApi: sourceApi);

    public ValueTask<Package> ExportPageRolesAsync(int appId, string sourceApi) =>
        corePackageDependency.ExportPageRolesAsync(
            appId: appId,
            sourceApi: sourceApi);

    public ValueTask<Package> ExportFolderRolesAsync(int appId, string sourceApi) =>
        corePackageDependency.ExportFolderRolesAsync(
            appId: appId,
            sourceApi: sourceApi);

    public Package ExportPackage(int appId, string packageName) =>
        corePackageDependency.ExportPackage(
            appId: appId,
            packageName: packageName);
}