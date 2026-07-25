// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Newtonsoft.Json;

namespace cCoder.IntegrationTests.Infrastructure;

internal static class AcceptanceSeedData
{
    public static Package[] LoadExportPackages()
    {
        using JsonDocument json = AcceptanceAssetLoader.LoadJson(fileName: "App.1.Export.json");
        JsonElement value = json.RootElement.GetProperty(propertyName: "value");

        return JsonConvert.DeserializeObject<Package[]>(
value:             value.GetRawText(),settings:             cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());
    }

    public static Role[] LoadRoles(string packageName, string itemType) =>
        [
            .. LoadPackageItems(packageName: packageName, itemType: itemType, modelType: typeof(Role))
                .Cast<Role>()
        ];

    public static Layout[] LoadLayouts(string packageName, string itemType) =>
        [
            .. LoadPackageItems(packageName: packageName, itemType: itemType, modelType: typeof(Layout))
                .Cast<Layout>()
        ];

    public static Template[] LoadTemplates(string packageName, string itemType) =>
        [
            .. LoadPackageItems(packageName: packageName, itemType: itemType, modelType: typeof(Template))
                .Cast<Template>()
        ];

    public static Resource[] LoadResources(string packageName, string itemType) =>
        [
            .. LoadPackageItems(packageName: packageName, itemType: itemType, modelType: typeof(Resource))
                .Cast<Resource>()
        ];

    public static Component[] LoadComponents(string packageName, string itemType) =>
        [
            .. LoadPackageItems(packageName: packageName, itemType: itemType, modelType: typeof(Component))
                .Cast<Component>()
        ];

    public static Script[] LoadScripts(string packageName, string itemType) =>
        [
            .. LoadPackageItems(packageName: packageName, itemType: itemType, modelType: typeof(Script))
                .Cast<Script>()
        ];

    private static object[] LoadPackageItems(string packageName, string itemType, Type modelType)
    {
        Package package = LoadExportPackages()
            .First(predicate: found =>
            string.Equals(a: found.Name,b: packageName,comparisonType: StringComparison.OrdinalIgnoreCase));

        return [.. package.Items
            .Where(predicate: item => string.Equals(a: item.Type,b: itemType,comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: item => UnpackItems(data: item.Data, modelType: modelType))];
    }

    public static CommonObject[] LoadCommonObjects()
    {
        List<CommonObject> result = [];

        result.AddRange(collection: LoadCommonObjects(fileName: "Core.Resource.latest.json"));
        result.AddRange(collection: LoadCommonObjects(fileName: "Core.Component.latest.json"));
        result.AddRange(collection: LoadCommonObjects(fileName: "Core.Script.latest.json"));

        return [.. result];
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

    private static IEnumerable<object> UnpackItems(string data, Type modelType)
    {
        string trimmed = data.TrimStart();

        if (trimmed.StartsWith(value: '['))
        {
            Array values = (Array)JsonConvert.DeserializeObject(
                value: trimmed,
                type: modelType.MakeArrayType(),
                settings: cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());

            return values.Cast<object>();
        }

        object value = JsonConvert.DeserializeObject(
            value: trimmed,
            type: modelType,
            settings: cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());

        return [value];
    }
}