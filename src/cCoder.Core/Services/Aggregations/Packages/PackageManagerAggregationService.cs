// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Packages;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Aggregations.Packages;

internal sealed partial class PackageManagerAggregationService(
    ICorePackageProcessingService corePackageProcessingService)
    : IPackageManagerAggregationService
{
    private const string AppConfigurationPackageName = "AppConfiguration";

    private static readonly string[] DefaultPackageNames =
    [
        AppConfigurationPackageName,
        "Roles",
        "Layouts",
        "Templates",
        "Resources",
        "Pages",
        "Workflows",
        "Components",
        "Scripts",
        "PageRoles",
        "FolderRoles",
        "Calendars",
        "CalendarEvents",
    ];

    public ValueTask<Package[]> ExportPackagesAsync(
        int appId,
        string[] packageNames,
        string sourceApi) =>
        TryCatch<Package[]>(operation: async () =>
        {
            ValidatePackagesOnExport(
                appId: appId,
                packageNames: packageNames,
                sourceApi: sourceApi);

            string[] requestedPackages = packageNames?
                .Where(predicate: packageName => !string.IsNullOrWhiteSpace(value: packageName))
                .ToArray() ?? [];

            if (requestedPackages.Length == 0)
            {
                requestedPackages = DefaultPackageNames;
            }

            List<Package> exportedPackages = [];

            foreach (string packageName in requestedPackages)
            {
                if (string.Equals(
                    a: packageName,
                    b: AppConfigurationPackageName,
                    comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    exportedPackages.Add(item: await corePackageProcessingService
                        .ExportAppConfigurationAsync(appId: appId, sourceApi: sourceApi));

                    continue;
                }

                if (string.Equals(
                    a: packageName,
                    b: "PageRoles",
                    comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    exportedPackages.Add(item: await corePackageProcessingService
                        .ExportPageRolesAsync(appId: appId, sourceApi: sourceApi));

                    continue;
                }

                if (string.Equals(
                    a: packageName,
                    b: "FolderRoles",
                    comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    exportedPackages.Add(item: await corePackageProcessingService
                        .ExportFolderRolesAsync(appId: appId, sourceApi: sourceApi));

                    continue;
                }

                exportedPackages.Add(item: corePackageProcessingService.ExportPackage(
                    appId: appId,
                    packageName: packageName));
            }

            return [.. exportedPackages];
        });
}