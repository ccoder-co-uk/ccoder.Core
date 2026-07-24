// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Newtonsoft.Json;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal static class AcceptanceSeedData
{
    public static Package[] LoadExportPackages()
    {
        using JsonDocument json = AcceptanceAssetLoader.LoadJson(fileName: "App.1.Export.json");
        JsonElement value = json.RootElement.GetProperty(propertyName: "value");

        return JsonConvert.DeserializeObject<Package[]>(
value: value.GetRawText(), settings: cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());
    }

    public static Role[] LoadRoles() =>
        LoadPackageItems(
            packageName: "Roles",
            itemTypeName: "Core/Role",
            itemType: typeof(Role))
        .Cast<Role>()
        .ToArray();

    public static Layout[] LoadLayouts() =>
        LoadPackageItems(
            packageName: "Layouts",
            itemTypeName: "Core/Layout",
            itemType: typeof(Layout))
        .Cast<Layout>()
        .ToArray();

    public static Template[] LoadTemplates() =>
        LoadPackageItems(
            packageName: "Templates",
            itemTypeName: "Core/Template",
            itemType: typeof(Template))
        .Cast<Template>()
        .ToArray();

    public static Resource[] LoadResources() =>
        LoadPackageItems(
            packageName: "Resources",
            itemTypeName: "Core/Resource",
            itemType: typeof(Resource))
        .Cast<Resource>()
        .ToArray();

    public static Component[] LoadComponents() =>
        LoadPackageItems(
            packageName: "Components",
            itemTypeName: "Core/Component",
            itemType: typeof(Component))
        .Cast<Component>()
        .ToArray();

    public static Script[] LoadScripts() =>
        LoadPackageItems(
            packageName: "Scripts",
            itemTypeName: "Core/Script",
            itemType: typeof(Script))
        .Cast<Script>()
        .ToArray();

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
value: value.GetRawText(), settings: cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());
    }

    private static object[] LoadPackageItems(
        string packageName,
        string itemTypeName,
        Type itemType)
    {
        Package package = LoadExportPackages()
            .First(predicate: found =>
                string.Equals(
                    a: found.Name,
                    b: packageName,
                    comparisonType: StringComparison.OrdinalIgnoreCase));

        return package.Items
            .Where(predicate: item =>
                string.Equals(
                    a: item.Type,
                    b: itemTypeName,
                    comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: item =>
                UnpackItems(
                    data: item.Data,
                    itemType: itemType))
            .ToArray();
    }

    private static IEnumerable<object> UnpackItems(
        string data,
        Type itemType)
    {
        string trimmed = data.TrimStart();
        Type deserializationType = trimmed.StartsWith(
            value: "[",
            comparisonType: StringComparison.Ordinal)
                ? itemType.MakeArrayType()
                : itemType;

        object deserialized = JsonConvert.DeserializeObject(
            value: trimmed,
            type: deserializationType,
            settings: cCoder.Data.Extensions.ObjectExtensions.GetJSONSettings());

        return deserialized is Array items
            ? items.Cast<object>()
            : [deserialized];
    }
}