// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Packaging;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Foundations.Packages;

internal sealed partial class CorePackageService(
    ICorePackageBroker corePackageBroker) : ICorePackageService
{
    public ValueTask ImportAppConfigurationAsync(int appId, string data) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOperation(appId: appId, value: data);

            return corePackageBroker.ImportAppConfigurationAsync(appId: appId, data: data);
        });

    public ValueTask<Package> ExportAppConfigurationAsync(int appId, string sourceApi) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOperation(appId: appId, value: sourceApi);

            return corePackageBroker.ExportAppConfigurationAsync(appId: appId, sourceApi: sourceApi);
        });

    public ValueTask<Package> ExportPageRolesAsync(int appId, string sourceApi) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOperation(appId: appId, value: sourceApi);

            return corePackageBroker.ExportPageRolesAsync(appId: appId, sourceApi: sourceApi);
        });

    public ValueTask<Package> ExportFolderRolesAsync(int appId, string sourceApi) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOperation(appId: appId, value: sourceApi);

            return corePackageBroker.ExportFolderRolesAsync(appId: appId, sourceApi: sourceApi);
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOperation(appId: appId, value: packageName);

            return corePackageBroker.ExportPackage(appId: appId, packageName: packageName);
        });
}