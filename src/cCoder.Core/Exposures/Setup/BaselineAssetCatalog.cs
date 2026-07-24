// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.Data.Extensions;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Newtonsoft.Json;
using AppSecurityUIBaseline = cCoder.AppSecurity.Exposures.Setup.UIBaseline;
using ContentManagementUIBaseline = cCoder.ContentManagement.Exposures.Setup.UIBaseline;
using DocumentManagementUIBaseline = cCoder.DocumentManagement.Exposures.Setup.UIBaseline;
using LoggingUIBaseline = cCoder.Logging.Exposures.Setup.UIBaseline;
using MailUIBaseline = cCoder.Mail.Exposures.Setup.UIBaseline;
using WorkflowUIBaseline = cCoder.Workflow.Exposures.Setup.UIBaseline;

namespace cCoder.Core.Exposures.Setup;

public sealed class BaselineAssetCatalog
{
    private const string ResourcePrefix = "cCoder.Core.Exposures.Setup.Assets.";

    private readonly Assembly assembly;
    private readonly JsonSerializerSettings settings = ObjectExtensions.GetJSONSettings();
    public BaselineAssetCatalog()
        : this(typeof(BaselineAssetCatalog).Assembly)
    {
    }

    internal BaselineAssetCatalog(Assembly assembly) =>
        this.assembly = assembly;

    public string LoadDefaultAppConfig() =>
        LoadText(relativePath: "DefaultAppConfig.json");

    public byte[] LoadAssetBytes(string relativePath) =>
        LoadBytes(relativePath: relativePath);

    public string[] LoadDmsAssetPaths() =>
        JsonConvert.DeserializeObject<string[]>(
value: LoadText(relativePath: Path.Combine(path1: "Baseline", path2: "DMS", path3: "BaselineDmsAssets.json")), settings: settings) ?? [];

    public Package[] LoadCoreReviewPackages() =>
        UIBaseline.Packages
            .Select(selector: ClonePackage)
            .Where(predicate: package => package.Items?.Count > 0)
            .ToArray();

    public Package[] LoadPackages() =>
        LoadBaselinePackages()
            .Select(selector: ClonePackage)
            .Where(predicate: package => package.Items?.Count > 0)
            .ToArray();

    public T[] LoadPackageItems<T>(string packageName, string itemType)
    {
        Package package = LoadBaselinePackages()
            .First(predicate: found =>
            string.Equals(a: found.Name, b: packageName, comparisonType: StringComparison.OrdinalIgnoreCase));

        return (package.Items ?? [])
            .Where(predicate: item => string.Equals(a: item.Type, b: itemType, comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: item => UnpackItems<T>(data: item.Data))
            .ToArray();
    }

    public CommonObject[] LoadCommonObjects() =>
        LoadPackageItems<Resource>(itemType: "Core/Resource")
            .Select(selector: ToCommonObject)
            .Concat(second: LoadPackageItems<Component>(itemType: "Core/Component")
                .Select(selector: ToCommonObject))
            .Concat(second: LoadPackageItems<Script>(itemType: "Core/Script")
                .Select(selector: ToCommonObject))
            .GroupBy(keySelector: item => $"{item.Type}\u001f{item.Key}\u001f{item.Culture}\u001f{item.Name}", comparer: StringComparer.OrdinalIgnoreCase)
            .Select(selector: group => group.First())
            .Select(selector: CloneCommonObject)
            .ToArray();

    private T[] LoadPackageItems<T>(string itemType) =>
        LoadBaselinePackages()
            .SelectMany(selector: package => package.Items ?? [])
            .Where(predicate: item => string.Equals(a: item.Type, b: itemType, comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: item => UnpackItems<T>(data: item.Data))
            .ToArray();

    private static IEnumerable<Package> LoadBaselinePackages() =>
        UIBaseline.Packages
            .Concat(AppSecurityUIBaseline.Packages)
            .Concat(ContentManagementUIBaseline.Packages)
            .Concat(DocumentManagementUIBaseline.Packages)
            .Concat(LoggingUIBaseline.Packages)
            .Concat(MailUIBaseline.Packages)
            .Concat(WorkflowUIBaseline.Packages);

    private string LoadText(string relativePath)
    {
        using Stream stream = LoadResourceStream(relativePath: relativePath);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private byte[] LoadBytes(string relativePath)
    {
        using Stream stream = LoadResourceStream(relativePath: relativePath);
        using MemoryStream memoryStream = new();
        stream.CopyTo(destination: memoryStream);
        return memoryStream.ToArray();
    }

    private Stream LoadResourceStream(string relativePath)
    {
        string resourceName = $"{ResourcePrefix}{relativePath.Replace(oldChar: '\\', newChar: '.')
            .Replace(oldChar: '/', newChar: '.')}";
        string normalizedResourceName = resourceName.Replace(oldChar: ' ', newChar: '_');

        return assembly.GetManifestResourceStream(name: resourceName)
            ?? assembly.GetManifestResourceStream(name: normalizedResourceName)
            ?? throw new FileNotFoundException($"Baseline asset was not found: {resourceName}", resourceName);
    }

    private static Package ClonePackage(Package package)
    {
        Guid packageId = Guid.NewGuid();
        PackageItem[] items = (package.Items ?? [])
            .Select(selector: item => ClonePackageItem(item: item, packageId: packageId))
            .ToArray();

        return new Package
        {
            Id = packageId,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = items,
        };
    }

    private static PackageItem ClonePackageItem(PackageItem item, Guid packageId) =>
        new()
        {
            Id = Guid.NewGuid(),
            PackageId = packageId,
            Type = item.Type,
            Data = item.Data,
        };

    private static CommonObject CloneCommonObject(CommonObject commonObject) =>
        CreateCommonObject(
name: commonObject.Name, description: commonObject.Description, lastUpdated: commonObject.LastUpdated, lastUpdatedBy: commonObject.LastUpdatedBy, createdOn: commonObject.CreatedOn, createdBy: commonObject.CreatedBy, version: commonObject.Version, key: commonObject.Key, type: commonObject.Type, json: commonObject.Json, culture: commonObject.Culture);

    private static CommonObject ToCommonObject(Resource resource) =>
        CreateCommonObject(
name: resource.Name, description: resource.Description, lastUpdated: resource.LastUpdated, lastUpdatedBy: resource.LastUpdatedBy, createdOn: resource.CreatedOn, createdBy: resource.CreatedBy, version: 1, key: resource.Key, type: "Core/Resource", json: JsonConvert.SerializeObject(value: resource, settings: ObjectExtensions.GetJSONSettings()), culture: resource.Culture);

    private static CommonObject ToCommonObject(Component component) =>
        CreateCommonObject(
name: component.Name, description: component.Description, lastUpdated: component.LastUpdated, lastUpdatedBy: component.LastUpdatedBy, createdOn: component.CreatedOn, createdBy: component.CreatedBy, version: 1, key: component.Key, type: "Core/Component", json: JsonConvert.SerializeObject(value: component, settings: ObjectExtensions.GetJSONSettings()), culture: string.Empty);

    private static CommonObject ToCommonObject(Script script) =>
        CreateCommonObject(
name: script.Name, description: script.Description, lastUpdated: script.LastUpdated, lastUpdatedBy: script.LastUpdatedBy, createdOn: script.CreatedOn, createdBy: script.CreatedBy, version: 1, key: script.Key, type: "Core/Script", json: JsonConvert.SerializeObject(value: script, settings: ObjectExtensions.GetJSONSettings()), culture: string.Empty);

    private static CommonObject CreateCommonObject(
        string name,
        string description,
        DateTimeOffset? lastUpdated,
        string lastUpdatedBy,
        DateTimeOffset? createdOn,
        string createdBy,
        int version,
        string key,
        string type,
        string json,
        string culture)
    {
        DateTimeOffset normalizedCreatedOn = createdOn ?? lastUpdated ?? DateTimeOffset.UtcNow;
        DateTimeOffset normalizedLastUpdated = lastUpdated ?? normalizedCreatedOn;
        string normalizedCreatedBy = string.IsNullOrWhiteSpace(value: createdBy) ? "setup" : createdBy;
        string normalizedLastUpdatedBy = string.IsNullOrWhiteSpace(value: lastUpdatedBy) ? normalizedCreatedBy : lastUpdatedBy;

        return new CommonObject
        {
            Id = 0,
            Name = name,
            Description = description,
            LastUpdated = normalizedLastUpdated,
            LastUpdatedBy = normalizedLastUpdatedBy,
            CreatedOn = normalizedCreatedOn,
            CreatedBy = normalizedCreatedBy,
            Version = version,
            Key = key,
            Type = type,
            Json = json,
            Culture = culture,
        };
    }

    private IEnumerable<T> UnpackItems<T>(string data)
    {
        string trimmed = data.TrimStart();

        return trimmed.StartsWith(value: "[", comparisonType: StringComparison.Ordinal)
            ? JsonConvert.DeserializeObject<T[]>(value: trimmed, settings: settings) ?? []
            : JsonConvert.DeserializeObject<T>(value: trimmed, settings: settings) is T item
                ? [item]
                : [];
    }
}