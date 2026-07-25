// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Extensions;
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

internal sealed partial class BaselinePackageCatalogProcessingService
    : IBaselinePackageCatalogProcessingService
{
    private readonly JsonSerializerSettings settings =
        ObjectExtensions.GetJSONSettings();

    public Package[] LoadCoreReviewPackages() =>
        TryCatch(operation: () =>
            CoreUIBaseline.GetPackages()
                .Select(selector: ClonePackage)
                .Where(predicate: package => package.Items?.Count > 0)
                .ToArray());

    public Package[] LoadPackages() =>
        TryCatch(operation: () =>
            LoadBaselinePackages()
                .Select(selector: ClonePackage)
                .Where(predicate: package => package.Items?.Count > 0)
                .ToArray());

    public T[] LoadPackageItems<T>(
        string packageName,
        string itemType) =>
        TryCatch(operation: () =>
        {
            ValidatePackageItemsOnLoad(
                packageName: packageName,
                itemType: itemType);

            Package package = LoadBaselinePackages()
                .First(predicate: found =>
                    string.Equals(
                        a: found.Name,
                        b: packageName,
                        comparisonType:
                            StringComparison.OrdinalIgnoreCase));

            return (package.Items ?? [])
                .Where(predicate: item =>
                    string.Equals(
                        a: item.Type,
                        b: itemType,
                        comparisonType:
                            StringComparison.OrdinalIgnoreCase))
                .SelectMany(selector: item =>
                    UnpackItems<T>(data: item.Data))
                .ToArray();
        });

    private static IEnumerable<Package> LoadBaselinePackages() =>
        CoreUIBaseline.GetPackages()
            .Concat(second: AppSecurityUIBaseline.Packages)
            .Concat(second: ContentManagementUIBaseline.Packages)
            .Concat(second: DocumentManagementUIBaseline.Packages)
            .Concat(second: LoggingUIBaseline.Packages)
            .Concat(second: MailUIBaseline.Packages)
            .Concat(second: WorkflowUIBaseline.Packages);

    private static Package ClonePackage(Package package)
    {
        Guid packageId = Guid.NewGuid();

        PackageItem[] items = (package.Items ?? [])
            .Select(selector: item =>
                ClonePackageItem(
                    item: item,
                    packageId: packageId))
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

    private static PackageItem ClonePackageItem(
        PackageItem item,
        Guid packageId) =>
        new()
        {
            Id = Guid.NewGuid(),
            PackageId = packageId,
            Type = item.Type,
            Data = item.Data,
        };

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