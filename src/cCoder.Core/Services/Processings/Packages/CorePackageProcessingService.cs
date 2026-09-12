// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.Packages;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class CorePackageProcessingService(
    ICorePackageService corePackageService) : ICorePackageProcessingService
{
    private const string AppConfigurationItemType = "Core/App";

    public ValueTask ImportPackageAsync(int appId, Package package) =>
        TryCatch(operation: async () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            PackageItem[] appItems =
            [
                .. (package.Items ?? []).Where(predicate: item => string.Equals(
                    a: item.Type,
                    b: AppConfigurationItemType,
                    comparisonType: StringComparison.OrdinalIgnoreCase))
            ];

            foreach (PackageItem appItem in appItems)
            {
                await corePackageService.ImportAppConfigurationAsync(
                    appId: appId,
                    data: appItem.Data);
            }
        });

    public ValueTask<Package> ExportAppConfigurationAsync(int appId, string sourceApi) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, value: sourceApi);
            return corePackageService.ExportAppConfigurationAsync(appId: appId, sourceApi: sourceApi);
        });

    public ValueTask<Package> ExportPageRolesAsync(int appId, string sourceApi) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, value: sourceApi);
            return corePackageService.ExportPageRolesAsync(appId: appId, sourceApi: sourceApi);
        });

    public ValueTask<Package> ExportFolderRolesAsync(int appId, string sourceApi) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, value: sourceApi);
            return corePackageService.ExportFolderRolesAsync(appId: appId, sourceApi: sourceApi);
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, value: packageName);
            return corePackageService.ExportPackage(appId: appId, packageName: packageName);
        });
}