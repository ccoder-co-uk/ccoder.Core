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
        LoadText("DefaultAppConfig.json");

    public byte[] LoadAssetBytes(string relativePath) =>
        LoadBytes(relativePath);

    public string[] LoadDmsAssetPaths() =>
        JsonConvert.DeserializeObject<string[]>(
            LoadText(Path.Combine("Baseline", "DMS", "BaselineDmsAssets.json")),
            settings) ?? [];

    public Package[] LoadCoreReviewPackages() =>
        UIBaseline.Packages
            .Select(ClonePackage)
            .Where(package => package.Items?.Count > 0)
            .ToArray();

    public Package[] LoadPackages() =>
        LoadBaselinePackages()
            .Select(ClonePackage)
            .Where(package => package.Items?.Count > 0)
            .ToArray();

    public T[] LoadPackageItems<T>(string packageName, string itemType)
    {
        Package package = LoadBaselinePackages().First(found =>
            string.Equals(found.Name, packageName, StringComparison.OrdinalIgnoreCase));

        return (package.Items ?? [])
            .Where(item => string.Equals(item.Type, itemType, StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => UnpackItems<T>(item.Data))
            .ToArray();
    }

    public CommonObject[] LoadCommonObjects() =>
        LoadPackageItems<Resource>("Core/Resource")
            .Select(ToCommonObject)
            .Concat(LoadPackageItems<Component>("Core/Component").Select(ToCommonObject))
            .Concat(LoadPackageItems<Script>("Core/Script").Select(ToCommonObject))
            .GroupBy(item => $"{item.Type}\u001f{item.Key}\u001f{item.Culture}\u001f{item.Name}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(CloneCommonObject)
            .ToArray();

    private T[] LoadPackageItems<T>(string itemType) =>
        LoadBaselinePackages()
            .SelectMany(package => package.Items ?? [])
            .Where(item => string.Equals(item.Type, itemType, StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => UnpackItems<T>(item.Data))
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
        using Stream stream = LoadResourceStream(relativePath);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private byte[] LoadBytes(string relativePath)
    {
        using Stream stream = LoadResourceStream(relativePath);
        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private Stream LoadResourceStream(string relativePath)
    {
        string resourceName = $"{ResourcePrefix}{relativePath.Replace('\\', '.').Replace('/', '.')}";
        string normalizedResourceName = resourceName.Replace(' ', '_');

        return assembly.GetManifestResourceStream(resourceName)
            ?? assembly.GetManifestResourceStream(normalizedResourceName)
            ?? throw new FileNotFoundException($"Baseline asset was not found: {resourceName}", resourceName);
    }

    private static Package ClonePackage(Package package)
    {
        Guid packageId = Guid.NewGuid();
        PackageItem[] items = (package.Items ?? [])
            .Select(item => ClonePackageItem(item, packageId))
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
            commonObject.Name,
            commonObject.Description,
            commonObject.LastUpdated,
            commonObject.LastUpdatedBy,
            commonObject.CreatedOn,
            commonObject.CreatedBy,
            commonObject.Version,
            commonObject.Key,
            commonObject.Type,
            commonObject.Json,
            commonObject.Culture);

    private static CommonObject ToCommonObject(Resource resource) =>
        CreateCommonObject(
            resource.Name,
            resource.Description,
            resource.LastUpdated,
            resource.LastUpdatedBy,
            resource.CreatedOn,
            resource.CreatedBy,
            1,
            resource.Key,
            "Core/Resource",
            JsonConvert.SerializeObject(resource, ObjectExtensions.GetJSONSettings()),
            resource.Culture);

    private static CommonObject ToCommonObject(Component component) =>
        CreateCommonObject(
            component.Name,
            component.Description,
            component.LastUpdated,
            component.LastUpdatedBy,
            component.CreatedOn,
            component.CreatedBy,
            1,
            component.Key,
            "Core/Component",
            JsonConvert.SerializeObject(component, ObjectExtensions.GetJSONSettings()),
            string.Empty);

    private static CommonObject ToCommonObject(Script script) =>
        CreateCommonObject(
            script.Name,
            script.Description,
            script.LastUpdated,
            script.LastUpdatedBy,
            script.CreatedOn,
            script.CreatedBy,
            1,
            script.Key,
            "Core/Script",
            JsonConvert.SerializeObject(script, ObjectExtensions.GetJSONSettings()),
            string.Empty);

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
        string normalizedCreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "setup" : createdBy;
        string normalizedLastUpdatedBy = string.IsNullOrWhiteSpace(lastUpdatedBy) ? normalizedCreatedBy : lastUpdatedBy;

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

        return trimmed.StartsWith("[", StringComparison.Ordinal)
            ? JsonConvert.DeserializeObject<T[]>(trimmed, settings) ?? []
            : JsonConvert.DeserializeObject<T>(trimmed, settings) is T item
                ? [item]
                : [];
    }
}
