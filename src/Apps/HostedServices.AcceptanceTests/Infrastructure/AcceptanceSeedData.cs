// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.Data.Models;
using cCoder.Data.Models.Packaging;
using Newtonsoft.Json;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal static class AcceptanceSeedData
{
    public static Package[] LoadExportPackages()
    {
        using JsonDocument json = AcceptanceAssetLoader.LoadJson(fileName: "App.1.Export.json");
        JsonElement value = json.RootElement.GetProperty(propertyName: "value");

        return JsonConvert.DeserializeObject<Package[]>(
value:             value.GetRawText(),settings:             cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());
    }

    public static T[] LoadPackageItems<T>(string packageName, string itemType)
    {
        Package package = LoadExportPackages()
            .First(predicate: found =>
            string.Equals(a: found.Name,b: packageName,comparisonType: StringComparison.OrdinalIgnoreCase));

        return package.Items
            .Where(predicate: item => string.Equals(a: item.Type,b: itemType,comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: item => UnpackItems<T>(data: item.Data))
            .ToArray();
    }

    public static CommonObject[] LoadCommonObjects()
    {
        List<CommonObject> result = [];

        result.AddRange(collection: LoadCommonObjects(fileName: "Core.Resource.latest.json"));
        result.AddRange(collection: LoadCommonObjects(fileName: "Core.Component.latest.json"));
        result.AddRange(collection: LoadCommonObjects(fileName: "Core.Script.latest.json"));

        return result.ToArray();
    }

    private static CommonObject[] LoadCommonObjects(string fileName)
    {
        using JsonDocument json = AcceptanceAssetLoader.LoadJson(fileName: fileName);

        JsonElement value =
            json.RootElement.ValueKind == JsonValueKind.Object
                ? json.RootElement.GetProperty(propertyName: "value")
                : json.RootElement;

        return JsonConvert.DeserializeObject<CommonObject[]>(
value:             value.GetRawText(),settings:             cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());
    }

    private static IEnumerable<T> UnpackItems<T>(string data)
    {
        string trimmed = data.TrimStart();

        return trimmed.StartsWith(value: "[",comparisonType: StringComparison.Ordinal)
            ? JsonConvert.DeserializeObject<T[]>(
value:                 trimmed,settings:                 cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings())
            : [
                JsonConvert.DeserializeObject<T>(
value:                     trimmed,settings:                     cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings())
            ];
    }
}