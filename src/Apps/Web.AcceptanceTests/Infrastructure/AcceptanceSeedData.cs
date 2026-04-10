using System.Text.Json;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.Packaging;
using Newtonsoft.Json;


namespace Web.AcceptanceTests.Infrastructure;

internal static class AcceptanceSeedData
{
    public static Package[] LoadExportPackages()
    {
        using JsonDocument json = AcceptanceAssetLoader.LoadJson("App.1.Export.json");
        JsonElement value = json.RootElement.GetProperty("value");
        return JsonConvert.DeserializeObject<Package[]>(
            value.GetRawText(),
            cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());
    }

    public static T[] LoadPackageItems<T>(string packageName, string itemType)
    {
        Package package = LoadExportPackages().First(found =>
            string.Equals(found.Name, packageName, StringComparison.OrdinalIgnoreCase)
        );

        return package.Items
            .Where(item => string.Equals(item.Type, itemType, StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => UnpackItems<T>(item.Data))
            .ToArray();
    }

    public static CommonObject[] LoadCommonObjects()
    {
        List<CommonObject> result = [];

        result.AddRange(LoadCommonObjects("Core.Resource.latest.json"));
        result.AddRange(LoadCommonObjects("Core.Component.latest.json"));
        result.AddRange(LoadCommonObjects("Core.Script.latest.json"));

        return result.ToArray();
    }

    private static CommonObject[] LoadCommonObjects(string fileName)
    {
        using JsonDocument json = AcceptanceAssetLoader.LoadJson(fileName);
        JsonElement value =
            json.RootElement.ValueKind == JsonValueKind.Object
                ? json.RootElement.GetProperty("value")
                : json.RootElement;

        return JsonConvert.DeserializeObject<CommonObject[]>(
            value.GetRawText(),
            cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());
    }

    private static IEnumerable<T> UnpackItems<T>(string data)
    {
        string trimmed = data.TrimStart();

        return trimmed.StartsWith("[", StringComparison.Ordinal)
            ? JsonConvert.DeserializeObject<T[]>(
                trimmed,
                cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings())
            : [
                JsonConvert.DeserializeObject<T>(
                    trimmed,
                    cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings())
            ];
    }
}






