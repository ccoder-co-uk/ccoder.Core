// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Extensions;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using Newtonsoft.Json;
using AppSecurityUIBaseline =
    cCoder.AppSecurity.Exposures.Setup.UIBaseline;
using ContentManagementUIBaseline =
    cCoder.ContentManagement.Exposures.Setup.UIBaseline;
using DocumentManagementUIBaseline =
    cCoder.DocumentManagement.Exposures.Setup.UIBaseline;
using LoggingUIBaseline =
    cCoder.Logging.Exposures.Setup.UIBaseline;
using MailUIBaseline =
    cCoder.Mail.Exposures.Setup.UIBaseline;
using WorkflowUIBaseline =
    cCoder.Workflow.Exposures.Setup.UIBaseline;
using CoreUIBaseline =
    cCoder.Core.Exposures.Setup.UIBaseline;

namespace cCoder.Core.Services.Processings.Setup;

internal sealed partial class
    BaselineCommonObjectCatalogProcessingService
        : IBaselineCommonObjectCatalogProcessingService
{
    private readonly JsonSerializerSettings settings =
        ObjectExtensions.GetJSONSettings();

    public CommonObject[] LoadCommonObjects() =>
        TryCatch(operation: () =>
            LoadPackageItems<Resource>(
                itemType: "Core/Resource")
                .Select(selector: ToCommonObject)
                .Concat(second:
                    LoadPackageItems<Component>(
                        itemType: "Core/Component")
                        .Select(selector: ToCommonObject))
                .Concat(second:
                    LoadPackageItems<Script>(
                        itemType: "Core/Script")
                        .Select(selector: ToCommonObject))
                .GroupBy(
                    keySelector: item =>
                        $"{item.Type}\u001f{item.Key}\u001f{item.Culture}\u001f{item.Name}",
                    comparer: StringComparer.OrdinalIgnoreCase)
                .Select(selector: group => group.First())
                .Select(selector: CloneCommonObject)
                .ToArray());

    private T[] LoadPackageItems<T>(string itemType) =>
        LoadBaselinePackages()
            .SelectMany(selector: package => package.Items ?? [])
            .Where(predicate: item =>
                string.Equals(
                    a: item.Type,
                    b: itemType,
                    comparisonType:
                        StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: item =>
                UnpackItems<T>(data: item.Data))
            .ToArray();

    private static IEnumerable<Package> LoadBaselinePackages() =>
        CoreUIBaseline.GetPackages()
            .Concat(second: AppSecurityUIBaseline.Packages)
            .Concat(second: ContentManagementUIBaseline.Packages)
            .Concat(second: DocumentManagementUIBaseline.Packages)
            .Concat(second: LoggingUIBaseline.Packages)
            .Concat(second: MailUIBaseline.Packages)
            .Concat(second: WorkflowUIBaseline.Packages);

    private static CommonObject CloneCommonObject(
        CommonObject commonObject) =>
        CreateCommonObject(
            name: commonObject.Name,
            description: commonObject.Description,
            lastUpdated: commonObject.LastUpdated,
            lastUpdatedBy: commonObject.LastUpdatedBy,
            createdOn: commonObject.CreatedOn,
            createdBy: commonObject.CreatedBy,
            version: commonObject.Version,
            key: commonObject.Key,
            type: commonObject.Type,
            json: commonObject.Json,
            culture: commonObject.Culture);

    private static CommonObject ToCommonObject(Resource resource) =>
        CreateCommonObject(
            name: resource.Name,
            description: resource.Description,
            lastUpdated: resource.LastUpdated,
            lastUpdatedBy: resource.LastUpdatedBy,
            createdOn: resource.CreatedOn,
            createdBy: resource.CreatedBy,
            version: 1,
            key: resource.Key,
            type: "Core/Resource",
            json: JsonConvert.SerializeObject(
                value: resource,
                settings: ObjectExtensions.GetJSONSettings()),
            culture: resource.Culture);

    private static CommonObject ToCommonObject(Component component) =>
        CreateCommonObject(
            name: component.Name,
            description: component.Description,
            lastUpdated: component.LastUpdated,
            lastUpdatedBy: component.LastUpdatedBy,
            createdOn: component.CreatedOn,
            createdBy: component.CreatedBy,
            version: 1,
            key: component.Key,
            type: "Core/Component",
            json: JsonConvert.SerializeObject(
                value: component,
                settings: ObjectExtensions.GetJSONSettings()),
            culture: string.Empty);

    private static CommonObject ToCommonObject(Script script) =>
        CreateCommonObject(
            name: script.Name,
            description: script.Description,
            lastUpdated: script.LastUpdated,
            lastUpdatedBy: script.LastUpdatedBy,
            createdOn: script.CreatedOn,
            createdBy: script.CreatedBy,
            version: 1,
            key: script.Key,
            type: "Core/Script",
            json: JsonConvert.SerializeObject(
                value: script,
                settings: ObjectExtensions.GetJSONSettings()),
            culture: string.Empty);

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
        DateTimeOffset normalizedCreatedOn =
            createdOn ?? lastUpdated ?? DateTimeOffset.UtcNow;

        DateTimeOffset normalizedLastUpdated =
            lastUpdated ?? normalizedCreatedOn;

        string normalizedCreatedBy =
            string.IsNullOrWhiteSpace(value: createdBy)
                ? "setup"
                : createdBy;

        string normalizedLastUpdatedBy =
            string.IsNullOrWhiteSpace(value: lastUpdatedBy)
                ? normalizedCreatedBy
                : lastUpdatedBy;

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

        return trimmed.StartsWith(
            value: "[",
            comparisonType: StringComparison.Ordinal)
                ? JsonConvert.DeserializeObject<T[]>(
                    value: trimmed,
                    settings: settings) ?? []
                : JsonConvert.DeserializeObject<T>(
                    value: trimmed,
                    settings: settings) is T item
                    ? [item]
                    : [];
    }
}