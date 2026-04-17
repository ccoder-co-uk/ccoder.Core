using cCoder.ContentManagement.Exposures.Caching;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Web.Models;

namespace Web.Services.Setup;

internal sealed class FirstTimeSetupAppService(
    FirstTimeSetupAssetService assetService,
    ICoreContextFactory coreContextFactory,
    IServiceProvider serviceProvider)
    : IFirstTimeSetupAppService
{
    private static readonly HashSet<string> ContentManagementTypes =
    [
        "Core/Layout",
        "Core/Template",
        "Core/Resource",
        "Core/Page",
        "Core/PageRole",
        "Core/Component",
        "Core/Script"
    ];

    private static readonly HashSet<string> WorkflowTypes =
    [
        "Core/FlowDefinition",
        "Core/FlowInstanceData",
        "Workflow/FlowDefinition",
        "Workflow/FlowInstanceData"
    ];

    private static readonly HashSet<string> SchedulingTypes =
    [
        "Core/Calendar",
        "Core/CalendarEvent",
        "Core/ScheduledTask",
        "Scheduling/Calendar",
        "Scheduling/CalendarEvent",
        "Scheduling/ScheduledTask"
    ];

    public async Task<App> CreateFirstAppAsync(
        FirstTimeSetupRequest request,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        Package[] packages = assetService.LoadPackages();
        CommonObject[] commonObjects = assetService.LoadCommonObjects();

        await EnsureGuestUserAsync(cancellationToken);

        IAppOrchestrationService appOrchestrationService =
            serviceProvider.GetRequiredService<IAppOrchestrationService>();

        App app = await appOrchestrationService.AddAsync(
            new App
            {
                Name = request.TenantName.Trim(),
                Domain = request.Domain,
                DefaultTheme = "Default",
                DefaultCultureId = string.Empty,
                TenantId = tenantId,
                ConfigJson = assetService.LoadDefaultAppConfig()
            });

        await ImportBaselinePackagesAsync(app.Id, packages);
        await PersistPackageCatalogAsync(packages, cancellationToken);
        await PersistCommonObjectsAsync(commonObjects, cancellationToken);

        ICommonObjectCache commonObjectCache =
            serviceProvider.GetRequiredService<ICommonObjectCache>();
        IMetadataCache metadataCache =
            serviceProvider.GetRequiredService<IMetadataCache>();
        commonObjectCache.Refresh();
        metadataCache.Rebuild();

        return app;
    }

    private async Task ImportBaselinePackagesAsync(int appId, IEnumerable<Package> packages)
    {
        cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker contentManagementPackageManagerBroker =
            serviceProvider.GetRequiredService<cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker>();
        cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker workflowPackageManagerBroker =
            serviceProvider.GetRequiredService<cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker>();
        cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker schedulingPackageManagerBroker =
            serviceProvider.GetRequiredService<cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker>();

        foreach (Package package in packages)
        {
            string[] itemTypes = (package.Items ?? [])
                .Select(item => item.Type)
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (itemTypes.Length == 0 || ContainsType(itemTypes, "Core/Role"))
                continue;

            if (itemTypes.Any(type => ContentManagementTypes.Contains(type)))
            {
                await contentManagementPackageManagerBroker.ImportPackageAsync(appId, package);
                continue;
            }

            if (itemTypes.Any(type => WorkflowTypes.Contains(type)))
            {
                await workflowPackageManagerBroker.ImportPackageAsync(appId, package);
                continue;
            }

            if (itemTypes.Any(type => SchedulingTypes.Contains(type)))
                await schedulingPackageManagerBroker.ImportPackageAsync(appId, package);
        }
    }

    private async Task EnsureGuestUserAsync(CancellationToken cancellationToken)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        bool exists = await core.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(user => user.Id == "Guest", cancellationToken);

        if (exists)
            return;

        await core.Set<User>().AddAsync(
            new User
            {
                Id = "Guest",
                Email = string.Empty,
                DisplayName = "Guest",
                DefaultCultureId = string.Empty,
                IsActive = true
            },
            cancellationToken);

        await core.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistPackageCatalogAsync(
        IEnumerable<Package> packages,
        CancellationToken cancellationToken)
    {
        Package[] clonedPackages = packages.ToArray();

        await using DbContext core = coreContextFactory.CreateCoreContext();
        await core.Set<Package>().AddRangeAsync(clonedPackages, cancellationToken);
        await core.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistCommonObjectsAsync(
        IEnumerable<CommonObject> commonObjects,
        CancellationToken cancellationToken)
    {
        CommonObject[] items = commonObjects.ToArray();
        NormalizeCommonObjects(items);

        await using DbContext core = coreContextFactory.CreateCoreContext();
        await core.Set<CommonObject>().AddRangeAsync(items, cancellationToken);
        await core.SaveChangesAsync(cancellationToken);
    }

    private static bool ContainsType(IEnumerable<string> itemTypes, string expectedType) =>
        itemTypes.Any(type => string.Equals(type, expectedType, StringComparison.OrdinalIgnoreCase));

    private static void NormalizeCommonObjects(IEnumerable<CommonObject> commonObjects)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (CommonObject commonObject in commonObjects)
        {
            NormalizeDateTimeOffsetProperty(commonObject, nameof(CommonObject.CreatedOn), now);
            NormalizeDateTimeOffsetProperty(commonObject, nameof(CommonObject.LastUpdated), now);
            NormalizeStringProperty(commonObject, nameof(CommonObject.CreatedBy), "setup");
            NormalizeStringProperty(commonObject, nameof(CommonObject.LastUpdatedBy), "setup");
        }
    }

    private static void NormalizeDateTimeOffsetProperty(CommonObject commonObject, string propertyName, DateTimeOffset fallbackValue)
    {
        PropertyInfo property = typeof(CommonObject).GetProperty(propertyName)!;
        object value = property.GetValue(commonObject);

        if (value is null)
        {
            property.SetValue(commonObject, fallbackValue);
            return;
        }

        if (value is DateTimeOffset dateTimeOffset && dateTimeOffset == default)
            property.SetValue(commonObject, fallbackValue);
    }

    private static void NormalizeStringProperty(CommonObject commonObject, string propertyName, string fallbackValue)
    {
        PropertyInfo property = typeof(CommonObject).GetProperty(propertyName)!;

        if (property.GetValue(commonObject) is not string value || string.IsNullOrWhiteSpace(value))
            property.SetValue(commonObject, fallbackValue);
    }

}
