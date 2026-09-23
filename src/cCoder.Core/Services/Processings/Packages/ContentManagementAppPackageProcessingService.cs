// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Json;
using cCoder.Core.Models.Packaging;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class ContentManagementAppPackageProcessingService(
    IContentManagementAppService contentManagementAppService,
    ICorePackageJsonBroker corePackageJsonBroker) : IContentManagementAppPackageProcessingService
{
    private const string AppConfigurationPackageName = "AppConfiguration";
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
                AppConfigurationPackageData data = corePackageJsonBroker.Deserialize(
                    data: appItem.Data);

                App app = contentManagementAppService.GetApp(
                    appId: appId,
                    ignoreFilters: true);

                app.DefaultCultureId = data.DefaultCultureId ?? string.Empty;
                app.Name = data.Name ?? app.Name;
                app.DefaultTheme = data.DefaultTheme ?? app.DefaultTheme;
                app.ConfigJson = data.ConfigJson ?? app.ConfigJson;

                await contentManagementAppService.UpdateAppAsync(updatedApp: app);
            }
        });

    public ValueTask<Package> ExportAppConfigurationAsync(int appId, string sourceApi) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, value: sourceApi);

            App app = contentManagementAppService.GetApp(
                appId: appId,
                ignoreFilters: true);

            AppConfigurationPackageData data = new()
            {
                Id = app.Id,
                DefaultCultureId = app.DefaultCultureId,
                TenantId = app.TenantId,
                Name = app.Name,
                Domain = app.Domain,
                DefaultTheme = app.DefaultTheme,
                ConfigJson = app.ConfigJson,
            };

            return ValueTask.FromResult(result: new Package
            {
                Name = AppConfigurationPackageName,
                Description = "Application shell configuration",
                Category = "Core",
                SourceApi = sourceApi,
                Items =
                [
                    new PackageItem
                    {
                        Type = AppConfigurationItemType,
                        Data = corePackageJsonBroker.Serialize(appConfigurationPackageData: data),
                    },
                ],
            });
        });

}