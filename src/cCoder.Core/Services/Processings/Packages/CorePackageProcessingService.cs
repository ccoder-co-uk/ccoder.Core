// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Nodes;
using cCoder.Core.Models.Packaging;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class CorePackageProcessingService(
    ICoreContextFactory coreContextFactory)
    : ICorePackageProcessingService
{
    private const string AppConfigurationItemType = "Core/App";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ValueTask ImportPackageAsync(
        int appId,
        Package package) =>
        TryCatch(operation: async () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);

            await ImportPackageCoreAsync(
                appId: appId,
                package: package);
        });

    private async ValueTask ImportPackageCoreAsync(
        int appId,
        Package package)
    {
        PackageItem[] appItems =
        [
            .. (package?.Items ?? []).Where(
                predicate: item => string.Equals(
                    a: item.Type,
                    b: AppConfigurationItemType,
                    comparisonType: StringComparison.OrdinalIgnoreCase))
        ];

        foreach (PackageItem appItem in appItems)
        {
            await ImportAppConfigurationPackageItemAsync(
                appId: appId,
                packageItem: appItem);
        }
    }

    private async ValueTask ImportAppConfigurationPackageItemAsync(
        int appId,
        PackageItem packageItem)
    {
        AppConfigurationPackageItem imported =
            DeserializeAppConfiguration(data: packageItem.Data);

        if (imported is null)
        {
            return;
        }

        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.Id == appId)
            ?? throw new InvalidOperationException(
                message: $"App '{appId}' was not found.");

        app.DefaultCultureId = imported.DefaultCultureId ?? string.Empty;
        app.Name = imported.Name ?? app.Name;
        app.DefaultTheme = imported.DefaultTheme ?? app.DefaultTheme;
        app.ConfigJson = imported.ConfigJson ?? app.ConfigJson;

        await core.SaveChangesAsync();
    }

    private static AppConfigurationPackageItem DeserializeAppConfiguration(
        string data)
    {
        if (string.IsNullOrWhiteSpace(value: data))
        {
            return null;
        }

        JsonNode node = JsonNode.Parse(json: data);

        if (node is null)
        {
            return null;
        }

        RemoveTypeMetadata(node: node);

        return node switch
        {
            JsonArray array => array.Deserialize<AppConfigurationPackageItem[]>(
                options: JsonOptions)?.FirstOrDefault(),
            JsonObject jsonObject => jsonObject.Deserialize<AppConfigurationPackageItem>(
                options: JsonOptions),
            _ => null,
        };
    }

    private static void RemoveTypeMetadata(JsonNode node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                jsonObject.Remove(propertyName: "$type");

                foreach (JsonNode child in jsonObject
                    .Select(selector: property => property.Value)
                    .Where(predicate: value => value is not null))
                {
                    RemoveTypeMetadata(node: child);
                }

                break;

            case JsonArray jsonArray:
                foreach (JsonNode child in jsonArray.Where(
                    predicate: value => value is not null))
                {
                    RemoveTypeMetadata(node: child);
                }

                break;
        }
    }
}