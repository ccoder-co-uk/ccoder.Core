// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures.Caching;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using cCoder.Core.Models;
using System.CodeDom.Compiler;
using cCoder.Core.Exposures.Setup;

namespace cCoder.Core.Services.Setup;

[GeneratedCode("decompilation-recovery", "1.0")]
internal sealed class FirstTimeSetupAppService(
    BaselineAssetCatalog assetService,
    ICoreContextFactory coreContextFactory,
    IServiceProvider serviceProvider)
    : IFirstTimeSetupAppService
{
    private static readonly HashSet<string> ContentManagementTypes =
    [
        "Core/Layout",
        "Core/Template",
        "Core/Page",
        "Core/PageRole",
        "Core/Component"
    ];

    private static readonly HashSet<string> CommonObjectOnlyTypes =
    [
        "Core/Resource",
        "Core/Script"
    ];

    private static readonly HashSet<string> AppScopedComponentNames =
    [
        "CoreManagement",
        "SSOMetadata",
        "SSORoleManagement",
        "SSORolePrivManagement",
        "SSORoleUserManagement"
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
        string firstAdminUserId = BuildUserId(email: request.Email);
        Package[] packages = assetService.LoadPackages();
        CommonObject[] commonObjects = assetService.LoadCommonObjects();
        NormalizeBaselinePackages(packages: packages, createdBy: firstAdminUserId);

        await EnsureGuestUserAsync(cancellationToken: cancellationToken);

        App app = await ResolveFirstAppAsync(
            request: request,
            tenantId: tenantId,
            cancellationToken: cancellationToken);

        await EnsureDefaultAppRolesAsync(appId: app.Id, firstAdminUserId: firstAdminUserId, cancellationToken: cancellationToken);
        await EnsureBootstrapAdminMembershipsAsync(appId: app.Id, userId: firstAdminUserId, cancellationToken: cancellationToken);
        await PersistBaselineFoldersAsync(appId: app.Id, packages: packages, cancellationToken: cancellationToken);
        await PersistBaselineDmsAssetsAsync(appId: app.Id, createdBy: firstAdminUserId, cancellationToken: cancellationToken);
        await ImportBaselinePackagesAsync(appId: app.Id, packages: packages);
        await PersistImportedPageVisibilityAsync(appId: app.Id, packages: packages, cancellationToken: cancellationToken);
        await PersistPackageCatalogAsync(packages: packages, cancellationToken: cancellationToken);
        await PersistCommonObjectsAsync(commonObjects: commonObjects, createdBy: firstAdminUserId, cancellationToken: cancellationToken);

        ICommonObjectCache commonObjectCache =
            serviceProvider.GetRequiredService<ICommonObjectCache>();

        IMetadataCache metadataCache =
            serviceProvider.GetRequiredService<IMetadataCache>();

        commonObjectCache.Refresh();
        metadataCache.Rebuild();

        return app;
    }

    public async Task RollbackAsync(
        string bootstrapUserId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(predicate: found =>
                found.TenantId == tenantId
                || found.Domain == tenantId, cancellationToken: cancellationToken);

        if (app is null)
        {
            return;
        }

        UserRole[] appUserRoles = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.UserId == bootstrapUserId)
            .ToArrayAsync(cancellationToken: cancellationToken);

        core.RemoveRange(entities: appUserRoles);
        core.Remove(entity: app);
        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private async Task<App> ResolveFirstAppAsync(
        FirstTimeSetupRequest request,
        string tenantId,
        CancellationToken cancellationToken)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        App existingApp = await core.Set<App>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(predicate: found =>
                found.Domain == request.Domain
                || (found.TenantId == tenantId && found.Name == request.TenantName.Trim()), cancellationToken: cancellationToken);

        if (existingApp is null)
        {
            existingApp = new App
            {
                Name = request.TenantName.Trim(),
                Domain = request.Domain,
                DefaultTheme = "Default",
                DefaultCultureId = string.Empty,
                TenantId = tenantId,
                ConfigJson = assetService.LoadDefaultAppConfig()
            };

            await core.Set<App>()
                .AddAsync(
                    entity: existingApp,
                    cancellationToken: cancellationToken);

            await core.SaveChangesAsync(
                cancellationToken: cancellationToken);

            return existingApp;
        }

        existingApp.Name = request.TenantName.Trim();
        existingApp.Domain = request.Domain;
        existingApp.DefaultTheme = "Default";
        existingApp.DefaultCultureId = string.Empty;
        existingApp.TenantId = tenantId;
        existingApp.ConfigJson = assetService.LoadDefaultAppConfig();

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
        return existingApp;
    }

    private async Task EnsureBootstrapAdminMembershipsAsync(
        int appId,
        string userId,
        CancellationToken cancellationToken)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        Role[] roles = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(predicate: role =>
                role.AppId == appId
                && (role.Name == "Administrators" || role.Name == "Users"))
            .ToArrayAsync(cancellationToken: cancellationToken);

        foreach (Role role in roles)
        {
            bool exists = await core.Set<UserRole>()
                .IgnoreQueryFilters()
                .AnyAsync(
predicate: userRole => userRole.RoleId == role.Id && userRole.UserId == userId, cancellationToken: cancellationToken);

            if (exists)
            {
                continue;
            }

            await core.Set<UserRole>()
                .AddAsync(
entity: new UserRole
{
    RoleId = role.Id,
    UserId = userId
}, cancellationToken: cancellationToken);
        }

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private async Task EnsureDefaultAppRolesAsync(
        int appId,
        string firstAdminUserId,
        CancellationToken cancellationToken)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        Privilege[] privileges = await core.Set<Privilege>()
            .IgnoreQueryFilters()
            .ToArrayAsync(cancellationToken: cancellationToken);

        string[] administratorPrivileges =
        [
            .. privileges
                .Select(selector: privilege => privilege.Id)
        ];

        string[] userPrivileges =
        [
            .. privileges
                .Where(predicate: privilege =>
                    string.Equals(a: privilege.Operation,b: "Read",comparisonType: StringComparison.OrdinalIgnoreCase)
                    && !IsWorkflowType(type: privilege.Type))
                .Select(selector: privilege => privilege.Id)
        ];

        Role administrators = await EnsureRoleAsync(
core: core, appId: appId, name: "Administrators", privileges: administratorPrivileges, cancellationToken: cancellationToken);

        Role users = await EnsureRoleAsync(
core: core, appId: appId, name: "Users", privileges: userPrivileges, cancellationToken: cancellationToken);

        Role guests = await EnsureRoleAsync(
core: core, appId: appId, name: "Guests", privileges: userPrivileges, cancellationToken: cancellationToken);

        await EnsureUserRoleAsync(core: core, roleId: administrators.Id, userId: firstAdminUserId, cancellationToken: cancellationToken);
        await EnsureUserRoleAsync(core: core, roleId: users.Id, userId: firstAdminUserId, cancellationToken: cancellationToken);
        await EnsureUserRoleAsync(core: core, roleId: guests.Id, userId: "Guest", cancellationToken: cancellationToken);
        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private static async Task<Role> EnsureRoleAsync(
        DbContext core,
        int appId,
        string name,
        IEnumerable<string> privileges,
        CancellationToken cancellationToken)
    {
        Role role = await core.Set<Role>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
predicate: found => found.AppId == appId && found.Name == name, cancellationToken: cancellationToken);

        if (role is null)
        {
            role = new Role
            {
                Id = Guid.NewGuid(),
                AppId = appId,
                Name = name,
                Privs = string.Empty
            };

            await core.Set<Role>()
                .AddAsync(entity: role, cancellationToken: cancellationToken);
        }

        role.Privs = JoinPrivileges(existingPrivileges: role.Privs, requiredPrivileges: privileges);
        return role;
    }

    private static async Task EnsureUserRoleAsync(
        DbContext core,
        Guid roleId,
        string userId,
        CancellationToken cancellationToken)
    {
        bool exists = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(
predicate: userRole => userRole.RoleId == roleId && userRole.UserId == userId, cancellationToken: cancellationToken);

        if (exists)
        {
            return;
        }

        await core.Set<UserRole>()
            .AddAsync(
entity: new UserRole
{
    RoleId = roleId,
    UserId = userId
}, cancellationToken: cancellationToken);
    }

    private static string JoinPrivileges(
        string existingPrivileges,
        IEnumerable<string> requiredPrivileges)
    {
        HashSet<string> privileges = new(
            (existingPrivileges ?? string.Empty)
                .Split(separator: ',', options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            StringComparer.OrdinalIgnoreCase);

        foreach (string privilege in requiredPrivileges)
        {
            privileges.Add(item: privilege);
        }

        return string.Join(separator: ',', values: privileges.OrderBy(keySelector: privilege => privilege, comparer: StringComparer.OrdinalIgnoreCase));
    }

    private static bool IsWorkflowType(string type) =>
        type.StartsWith(value: "Flow", comparisonType: StringComparison.OrdinalIgnoreCase)
        || type.StartsWith(value: "Workflow", comparisonType: StringComparison.OrdinalIgnoreCase);

    private async Task ImportBaselinePackagesAsync(int appId, IEnumerable<Package> packages)
    {
        cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker workflowPackageManagerBroker =
            serviceProvider.GetRequiredService<cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker>();

        cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker schedulingPackageManagerBroker =
            serviceProvider.GetRequiredService<cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker>();

        foreach (Package package in packages)
        {
            string[] itemTypes = (package.Items ?? [])
                .Select(selector: item => item.Type)
                .Where(predicate: type => !string.IsNullOrWhiteSpace(value: type))
                .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Package importPackage = CreateAppImportPackage(package: package);

            itemTypes = (importPackage.Items ?? [])
                .Select(selector: item => item.Type)
                .Where(predicate: type => !string.IsNullOrWhiteSpace(value: type))
                .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (itemTypes.Length == 0 || ContainsType(itemTypes: itemTypes, expectedType: "Core/Role"))
            {
                continue;
            }

            if (itemTypes.Any(predicate: type => ContentManagementTypes.Contains(item: type)))
            {
                await ImportContentManagementPackageAsync(appId: appId, package: importPackage);
                continue;
            }

            if (itemTypes.Any(predicate: type => WorkflowTypes.Contains(item: type)))
            {
                await workflowPackageManagerBroker.ImportPackageAsync(appId: appId, package: importPackage);
                continue;
            }

            if (itemTypes.Any(predicate: type => SchedulingTypes.Contains(item: type)))
            {
                await schedulingPackageManagerBroker.ImportPackageAsync(appId: appId, package: importPackage);
            }
        }
    }

    private async Task ImportContentManagementPackageAsync(int appId, Package package)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        foreach (PackageItem item in package.Items ?? [])
        {
            switch (item.Type)
            {
                case "Core/Component":
                    await PersistComponentsAsync(
core: core, appId: appId, components: UnpackPackageItem<Component>(data: item.Data));
                    break;
                case "Core/Layout":
                    await PersistLayoutsAsync(
core: core, appId: appId, layouts: UnpackPackageItem<Layout>(data: item.Data));
                    break;
                case "Core/Page":
                    await PersistPagesAsync(
core: core, appId: appId, pages: UnpackPackageItem<Page>(data: item.Data));
                    break;
                case "Core/PageRole":
                    await PersistPageRolesAsync(
core: core, appId: appId, pageRoles: UnpackPackageItem<cCoder.ContentManagement.Models.PageRoleInfo>(data: item.Data));
                    break;
                case "Core/Template":
                    await PersistTemplatesAsync(
core: core, appId: appId, templates: UnpackPackageItem<Template>(data: item.Data));
                    break;
            }
        }
    }

    private static T[] UnpackPackageItem<T>(string data)
    {
        string trimmed = data.TrimStart();

        return trimmed.StartsWith(value: '[')
            ? JsonConvert.DeserializeObject<T[]>(value: trimmed) ?? []
            : JsonConvert.DeserializeObject<T>(value: trimmed) is T item
                ? [item]
                : [];
    }

    private static async Task PersistLayoutsAsync(DbContext core, int appId, IEnumerable<Layout> layouts)
    {
        Layout[] items = layouts.ToArray();

        if (items.Length == 0)
        {
            return;
        }

        Layout[] existingLayouts = await core.Set<Layout>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToArrayAsync();

        foreach (Layout item in items)
        {
            Layout existingLayout = existingLayouts.FirstOrDefault(predicate: found =>
                string.Equals(a: found.Name, b: item.Name, comparisonType: StringComparison.OrdinalIgnoreCase));

            if (existingLayout is null)
            {
                await core.Set<Layout>()
                    .AddAsync(entity: new Layout
                {
                    AppId = appId,
                    Name = item.Name,
                    Description = item.Description,
                    HeaderHtml = item.HeaderHtml,
                    Html = item.Html,
                    Script = item.Script,
                    CreatedBy = item.CreatedBy,
                    CreatedOn = item.CreatedOn,
                    LastUpdated = item.LastUpdated,
                    LastUpdatedBy = item.LastUpdatedBy,
                });

                continue;
            }

            existingLayout.Description = item.Description;
            existingLayout.HeaderHtml = item.HeaderHtml;
            existingLayout.Html = item.Html;
            existingLayout.Script = item.Script;
            existingLayout.CreatedBy = item.CreatedBy;
            existingLayout.CreatedOn = item.CreatedOn;
            existingLayout.LastUpdated = item.LastUpdated;
            existingLayout.LastUpdatedBy = item.LastUpdatedBy;
        }

        await core.SaveChangesAsync();
    }

    private static async Task PersistTemplatesAsync(DbContext core, int appId, IEnumerable<Template> templates)
    {
        Template[] items = templates.ToArray();

        if (items.Length == 0)
        {
            return;
        }

        Template[] existingTemplates = await core.Set<Template>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToArrayAsync();

        foreach (Template item in items)
        {
            Template existingTemplate = existingTemplates.FirstOrDefault(predicate: found =>
                string.Equals(a: found.Name, b: item.Name, comparisonType: StringComparison.OrdinalIgnoreCase));

            if (existingTemplate is null)
            {
                await core.Set<Template>()
                    .AddAsync(entity: new Template
                {
                    AppId = appId,
                    Name = item.Name,
                    Description = item.Description,
                    ResourceKey = item.ResourceKey,
                    RawString = item.RawString,
                    CreatedBy = item.CreatedBy,
                    CreatedOn = item.CreatedOn,
                    LastUpdated = item.LastUpdated,
                    LastUpdatedBy = item.LastUpdatedBy,
                });

                continue;
            }

            existingTemplate.Description = item.Description;
            existingTemplate.ResourceKey = item.ResourceKey;
            existingTemplate.RawString = item.RawString;
            existingTemplate.CreatedBy = item.CreatedBy;
            existingTemplate.CreatedOn = item.CreatedOn;
            existingTemplate.LastUpdated = item.LastUpdated;
            existingTemplate.LastUpdatedBy = item.LastUpdatedBy;
        }

        await core.SaveChangesAsync();
    }

    private static async Task PersistComponentsAsync(DbContext core, int appId, IEnumerable<Component> components)
    {
        Component[] items = components.ToArray();

        if (items.Length == 0)
        {
            return;
        }

        Component[] existingComponents = await core.Set<Component>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToArrayAsync();

        foreach (Component item in items)
        {
            Component existingComponent = existingComponents.FirstOrDefault(predicate: found =>
                string.Equals(a: found.Name, b: item.Name, comparisonType: StringComparison.OrdinalIgnoreCase));

            if (existingComponent is null)
            {
                await core.Set<Component>()
                    .AddAsync(entity: new Component
                {
                    AppId = appId,
                    Name = item.Name,
                    Description = item.Description,
                    ResourceKey = item.ResourceKey,
                    Content = item.Content,
                    Script = item.Script,
                    Key = item.Key,
                    CreatedBy = item.CreatedBy,
                    CreatedOn = item.CreatedOn,
                    LastUpdated = item.LastUpdated,
                    LastUpdatedBy = item.LastUpdatedBy,
                });

                continue;
            }

            existingComponent.Description = item.Description;
            existingComponent.ResourceKey = item.ResourceKey;
            existingComponent.Content = item.Content;
            existingComponent.Script = item.Script;
            existingComponent.Key = item.Key;
            existingComponent.CreatedBy = item.CreatedBy;
            existingComponent.CreatedOn = item.CreatedOn;
            existingComponent.LastUpdated = item.LastUpdated;
            existingComponent.LastUpdatedBy = item.LastUpdatedBy;
        }

        await core.SaveChangesAsync();
    }

    private static async Task PersistPagesAsync(DbContext core, int appId, IEnumerable<Page> pages)
    {
        Page[] items = pages
            .OrderBy(keySelector: item => GetPageDepth(path: item.Path))
            .ThenBy(keySelector: item => item.Order)
            .ToArray();

        if (items.Length == 0)
        {
            return;
        }

        foreach (Page item in items)
        {
            string normalizedPath = NormalizePagePath(path: item.Path);
            string parentPath = GetParentPagePath(path: normalizedPath);

            Page existingPage = await core.Set<Page>()
                .IgnoreQueryFilters()
                .Include(navigationPropertyPath: found => found.PageInfo)
                .Include(navigationPropertyPath: found => found.Contents)
                .FirstOrDefaultAsync(predicate: found =>
                    found.AppId == appId &&
                    found.Path == normalizedPath);

            Page parent = string.IsNullOrWhiteSpace(value: parentPath)
                ? null
                : await core.Set<Page>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(predicate: found =>
                        found.AppId == appId &&
                        found.Path == parentPath);

            if (existingPage is null)
            {
                Page newPage = new()
                {
                    AppId = appId,
                    ParentId = parent?.Id,
                    Order = item.Order,
                    ShowOnMenus = item.ShowOnMenus,
                    Name = item.Name,
                    LastUpdated = item.LastUpdated,
                    LastUpdatedBy = item.LastUpdatedBy,
                    CreatedOn = item.CreatedOn,
                    CreatedBy = item.CreatedBy,
                    Path = normalizedPath,
                    ResourceKey = item.ResourceKey,
                    Layout = item.Layout,
                    PageInfo = (item.PageInfo ?? [])
                        .Select(selector: info => new PageInfo
                        {
                            CultureId = info.CultureId,
                            Title = info.Title,
                            Description = info.Description,
                            Keywords = info.Keywords,
                        })
                        .ToList(),
                    Contents = (item.Contents ?? [])
                        .Select(selector: content => new cCoder.Data.Models.CMS.Content
                        {
                            CultureId = content.CultureId,
                            Name = content.Name,
                            Html = content.Html,
                        })
                        .ToList(),
                };

                await core.Set<Page>()
                    .AddAsync(entity: newPage);

                await core.SaveChangesAsync();
                continue;
            }

            existingPage.ParentId = parent?.Id;
            existingPage.Order = item.Order;
            existingPage.ShowOnMenus = item.ShowOnMenus;
            existingPage.Name = item.Name;
            existingPage.LastUpdated = item.LastUpdated;
            existingPage.LastUpdatedBy = item.LastUpdatedBy;
            existingPage.CreatedOn = item.CreatedOn;
            existingPage.CreatedBy = item.CreatedBy;
            existingPage.Path = normalizedPath;
            existingPage.ResourceKey = item.ResourceKey;
            existingPage.Layout = item.Layout;

            core.Set<PageInfo>()
                .RemoveRange(entities: existingPage.PageInfo ?? []);

            core.Set<cCoder.Data.Models.CMS.Content>()
                .RemoveRange(entities: existingPage.Contents ?? []);

            existingPage.PageInfo = (item.PageInfo ?? [])
                .Select(selector: info => new PageInfo
                {
                    PageId = existingPage.Id,
                    CultureId = info.CultureId,
                    Title = info.Title,
                    Description = info.Description,
                    Keywords = info.Keywords,
                })
                .ToList();

            existingPage.Contents = (item.Contents ?? [])
                .Select(selector: content => new cCoder.Data.Models.CMS.Content
                {
                    PageId = existingPage.Id,
                    CultureId = content.CultureId,
                    Name = content.Name,
                    Html = content.Html,
                })
                .ToList();

            await core.SaveChangesAsync();
        }
    }

    private static async Task PersistPageRolesAsync(
        DbContext core,
        int appId,
        IEnumerable<cCoder.ContentManagement.Models.PageRoleInfo> pageRoles)
    {
        cCoder.ContentManagement.Models.PageRoleInfo[] items = pageRoles.ToArray();

        if (items.Length == 0)
        {
            return;
        }

        Dictionary<string, int> pageIdsByPath = await core.Set<Page>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToDictionaryAsync(keySelector: found => found.Path, elementSelector: found => found.Id, comparer: StringComparer.OrdinalIgnoreCase);

        Role[] roles = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToArrayAsync();

        Dictionary<string, Guid> roleIdsByName = roles
            .GroupBy(
                keySelector: found => found.Name,
                comparer: StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                keySelector: group => group.Key,
                elementSelector: group => group.First().Id,
                comparer: StringComparer.OrdinalIgnoreCase);

        HashSet<string> existingPairs =
        [
            .. await core.Set<PageRole>()
                .IgnoreQueryFilters()
                .Where(predicate: found => pageIdsByPath.Values.Contains(value: found.PageId))
                .Select(selector: found => found.PageId + "|" + found.RoleId)
                .ToArrayAsync()
        ];

        foreach (cCoder.ContentManagement.Models.PageRoleInfo item in items)
        {
            string normalizedPath = NormalizePagePath(path: item.Path);

            if (!pageIdsByPath.TryGetValue(key: normalizedPath, value: out int pageId))
            {
                throw new InvalidOperationException($"Baseline page role target page was not imported: {normalizedPath}");
            }

            if (!roleIdsByName.TryGetValue(key: item.Role, value: out Guid roleId))
            {
                throw new InvalidOperationException($"Baseline page role target role was not found: {item.Role}");
            }

            string key = pageId + "|" + roleId;

            if (!existingPairs.Add(item: key))
            {
                continue;
            }

            await core.Set<PageRole>()
                .AddAsync(entity: new PageRole
            {
                PageId = pageId,
                RoleId = roleId,
            });
        }

        await core.SaveChangesAsync();
    }

    private async Task EnsureGuestUserAsync(CancellationToken cancellationToken)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        bool exists = await core.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: user => user.Id == "Guest", cancellationToken: cancellationToken);

        if (exists)
        {
            return;
        }

        await core.Set<User>()
            .AddAsync(
entity: new User
{
    Id = "Guest",
    Email = string.Empty,
    DisplayName = "Guest",
    DefaultCultureId = string.Empty,
    IsActive = true
}, cancellationToken: cancellationToken);

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private async Task PersistPackageCatalogAsync(
        IEnumerable<Package> packages,
        CancellationToken cancellationToken)
    {
        Package[] clonedPackages = packages.ToArray();

        await using DbContext core = coreContextFactory.CreateCoreContext();

        string[] packageNames = clonedPackages
            .Select(selector: package => package.Name)
            .Where(predicate: name => !string.IsNullOrWhiteSpace(value: name))
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Package[] existingPackages = await core.Set<Package>()
            .IgnoreQueryFilters()
            .Include(navigationPropertyPath: found => found.Items)
            .Where(predicate: found => packageNames.Contains(value: found.Name))
            .ToArrayAsync(cancellationToken: cancellationToken);

        foreach (Package package in clonedPackages)
        {
            Package existingPackage = existingPackages.FirstOrDefault(predicate: found =>
                string.Equals(a: found.Name, b: package.Name, comparisonType: StringComparison.OrdinalIgnoreCase));

            if (existingPackage is null)
            {
                await core.Set<Package>()
                    .AddAsync(entity: package, cancellationToken: cancellationToken);

                continue;
            }

            existingPackage.Description = package.Description;
            existingPackage.Category = package.Category;
            existingPackage.SourceApi = package.SourceApi;

            core.Set<PackageItem>()
                .RemoveRange(entities: existingPackage.Items ?? []);

            existingPackage.Items = (package.Items ?? [])
                .Select(selector: item => new PackageItem
                {
                    Id = Guid.NewGuid(),
                    PackageId = existingPackage.Id,
                    Type = item.Type,
                    Data = item.Data,
                })
                .ToArray();
        }

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private async Task PersistBaselineFoldersAsync(
        int appId,
        IEnumerable<Package> packages,
        CancellationToken cancellationToken)
    {
        string[] paths = packages
            .SelectMany(selector: package => package.Items ?? [])
            .Where(predicate: item => string.Equals(a: item.Type, b: "Core/FolderRole", comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: item => ExtractFolderRolePaths(data: item.Data))
            .Where(predicate: path => !string.IsNullOrWhiteSpace(value: path))
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
            .OrderBy(keySelector: path => path.Count(predicate: character => character == '/'))
            .ThenBy(keySelector: path => path, comparer: StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (paths.Length == 0)
        {
            return;
        }

        await using DbContext core = coreContextFactory.CreateCoreContext();

        Folder[] existingFolders = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(predicate: folder => folder.AppId == appId)
            .ToArrayAsync(cancellationToken: cancellationToken);

        Dictionary<string, Folder> foldersByPath = existingFolders
            .Where(predicate: folder => !string.IsNullOrWhiteSpace(value: folder.Path))
            .ToDictionary(keySelector: folder => folder.Path, comparer: StringComparer.OrdinalIgnoreCase);

        foreach (string path in paths)
        {
            if (foldersByPath.ContainsKey(key: path))
            {
                continue;
            }

            string parentPath = GetParentFolderPath(path: path);

            Folder folder = new()
            {
                Id = Guid.NewGuid(),
                AppId = appId,
                ParentId = !string.IsNullOrWhiteSpace(value: parentPath)
                    && foldersByPath.TryGetValue(key: parentPath, value: out Folder parent)
                        ? parent.Id
                        : null,
                Name = GetFolderName(path: path),
                Path = path,
            };

            foldersByPath[path] = folder;

            await core.Set<Folder>()
                .AddAsync(entity: folder, cancellationToken: cancellationToken);
        }

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private async Task PersistBaselineDmsAssetsAsync(
        int appId,
        string createdBy,
        CancellationToken cancellationToken)
    {
        string[] assetPaths = assetService.LoadDmsAssetPaths()
            .Where(predicate: path => !string.IsNullOrWhiteSpace(value: path))
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
            .OrderBy(keySelector: path => path.Count(predicate: character => character is '/' or '\\'))
            .ThenBy(keySelector: path => path, comparer: StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (assetPaths.Length == 0)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        await using DbContext core = coreContextFactory.CreateCoreContext();

        Folder[] existingFolders = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(predicate: folder => folder.AppId == appId)
            .ToArrayAsync(cancellationToken: cancellationToken);

        Dictionary<string, Folder> foldersByPath = existingFolders
            .Where(predicate: folder => !string.IsNullOrWhiteSpace(value: folder.Path))
            .ToDictionary(keySelector: folder => folder.Path.ToLowerInvariant(), comparer: StringComparer.OrdinalIgnoreCase);

        string[] filePaths = assetPaths
            .Select(selector: GetBaselineDmsPath)
            .Select(selector: path => path.ToLowerInvariant())
            .ToArray();

        cCoder.Data.Models.DMS.File[] existingFiles = await core.Set<cCoder.Data.Models.DMS.File>()
            .IgnoreQueryFilters()
            .Include(navigationPropertyPath: found => found.Contents)
            .Where(predicate: file => filePaths.Contains(value: file.Path))
            .ToArrayAsync(cancellationToken: cancellationToken);

        Dictionary<string, cCoder.Data.Models.DMS.File> filesByPath = existingFiles
            .ToDictionary(keySelector: file => file.Path, comparer: StringComparer.OrdinalIgnoreCase);

        foreach (string assetPath in assetPaths)
        {
            byte[] assetBytes = assetService.LoadAssetBytes(relativePath: assetPath);
            string dmsPath = GetBaselineDmsPath(assetPath: assetPath);
            string filePath = dmsPath.ToLowerInvariant();
            string folderPath = GetParentFolderPath(path: filePath);
            string fileName = GetFolderName(path: dmsPath);
            string fileSize = GetSizeOf(content: assetBytes);
            Folder folder = await EnsureFolderAsync(core: core, foldersByPath: foldersByPath, appId: appId, path: folderPath, cancellationToken: cancellationToken);

            if (!filesByPath.TryGetValue(key: filePath, value: out cCoder.Data.Models.DMS.File file))
            {
                file = new cCoder.Data.Models.DMS.File
                {
                    Id = Guid.NewGuid(),
                    FolderId = folder.Id,
                    Folder = folder,
                    Name = fileName,
                    Path = filePath,
                    MimeType = GetMimeType(fileName: fileName),
                    Size = fileSize,
                    CreatedBy = createdBy,
                    CreatedOn = now,
                    Contents = [],
                };

                filesByPath[filePath] = file;

                await core.Set<cCoder.Data.Models.DMS.File>()
                    .AddAsync(entity: file, cancellationToken: cancellationToken);
            }
            else
            {
                file.FolderId = folder.Id;
                file.Folder = folder;
                file.Name = fileName;
                file.MimeType = GetMimeType(fileName: fileName);
                file.Size = fileSize;
            }

            FileContent content = file.Contents
                .OrderByDescending(keySelector: found => found.Version)
                .FirstOrDefault();

            if (content is null)
            {
                file.Contents.Add(
item: new FileContent
{
    Id = Guid.NewGuid(),
    FileId = file.Id,
    File = file,
    Description = "Baseline DMS asset",
    Size = fileSize,
    CreatedBy = createdBy,
    CreatedOn = now,
    Version = 1,
    RawData = assetBytes,
});
            }
            else
            {
                content.Description = "Baseline DMS asset";
                content.Size = fileSize;
                content.RawData = assetBytes;
                content.CreatedBy = string.IsNullOrWhiteSpace(value: content.CreatedBy) ? createdBy : content.CreatedBy;
                content.CreatedOn = content.CreatedOn == default ? now : content.CreatedOn;
            }
        }

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private static async Task<Folder> EnsureFolderAsync(
        DbContext core,
        Dictionary<string, Folder> foldersByPath,
        int appId,
        string path,
        CancellationToken cancellationToken)
    {
        string normalizedPath = path.Trim()
            .Trim(trimChar: '/')
            .ToLowerInvariant();

        if (foldersByPath.TryGetValue(key: normalizedPath, value: out Folder existingFolder))
        {
            return existingFolder;
        }

        string parentPath = GetParentFolderPath(path: normalizedPath);

        Folder parent = string.IsNullOrWhiteSpace(value: parentPath)
            ? null
            : await EnsureFolderAsync(core: core, foldersByPath: foldersByPath, appId: appId, path: parentPath, cancellationToken: cancellationToken);

        Folder folder = new()
        {
            Id = Guid.NewGuid(),
            AppId = appId,
            ParentId = parent?.Id,
            Parent = parent,
            Name = GetFolderName(path: normalizedPath),
            Path = normalizedPath,
        };

        foldersByPath[normalizedPath] = folder;

        await core.Set<Folder>()
            .AddAsync(entity: folder, cancellationToken: cancellationToken);

        return folder;
    }

    private static string GetBaselineDmsPath(string assetPath)
    {
        const string prefix = "Baseline/DMS/";

        string normalizedPath = assetPath.Replace(oldChar: '\\', newChar: '/')
            .Trim(trimChar: '/');

        if (!normalizedPath.StartsWith(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"DMS baseline asset path must start with {prefix}: {assetPath}");
        }

        return normalizedPath[prefix.Length..];
    }

    private static string GetMimeType(string fileName)
    {
        string extension = Path.GetExtension(path: fileName);

        return extension.ToLowerInvariant() switch
        {
            ".gif" => "image/gif",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };
    }

    private async Task PersistImportedPageVisibilityAsync(
        int appId,
        IEnumerable<Package> packages,
        CancellationToken cancellationToken)
    {
        Dictionary<string, bool> visibilityByPath = packages
            .SelectMany(selector: package => package.Items ?? [])
            .Where(predicate: item => string.Equals(a: item.Type, b: "Core/Page", comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: item => ExtractPageVisibility(data: item.Data))
            .GroupBy(keySelector: item => item.Path, comparer: StringComparer.OrdinalIgnoreCase)
            .ToDictionary(keySelector: group => group.Key, elementSelector: group => group.Last().ShowOnMenus, comparer: StringComparer.OrdinalIgnoreCase);

        if (visibilityByPath.Count == 0)
        {
            return;
        }

        await using DbContext core = coreContextFactory.CreateCoreContext();

        Page[] pages = await core.Set<Page>()
            .IgnoreQueryFilters()
            .Where(predicate: page => page.AppId == appId)
            .ToArrayAsync(cancellationToken: cancellationToken);

        foreach (Page page in pages)
        {
            string normalizedPath = NormalizePagePath(path: page.Path);

            if (visibilityByPath.TryGetValue(key: normalizedPath, value: out bool showOnMenus))
            {
                page.ShowOnMenus = showOnMenus;
            }
        }

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private async Task PersistCommonObjectsAsync(
        IEnumerable<CommonObject> commonObjects,
        string createdBy,
        CancellationToken cancellationToken)
    {
        CommonObject[] items = commonObjects.ToArray();
        NormalizeCommonObjects(commonObjects: items, createdBy: createdBy);

        await using DbContext core = coreContextFactory.CreateCoreContext();

        string[] names = items
            .Select(selector: item => item.Name)
            .Where(predicate: name => !string.IsNullOrWhiteSpace(value: name))
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
            .ToArray();

        CommonObject[] existingItems = await core.Set<CommonObject>()
            .IgnoreQueryFilters()
            .Where(predicate: found => names.Contains(value: found.Name))
            .ToArrayAsync(cancellationToken: cancellationToken);

        foreach (CommonObject item in items)
        {
            CommonObject existingItem = existingItems.FirstOrDefault(predicate: found =>
                string.Equals(a: found.Name, b: item.Name, comparisonType: StringComparison.OrdinalIgnoreCase)
                && string.Equals(a: found.Type, b: item.Type, comparisonType: StringComparison.OrdinalIgnoreCase)
                && string.Equals(a: found.Key ?? string.Empty, b: item.Key ?? string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase)
                && string.Equals(a: found.Culture ?? string.Empty, b: item.Culture ?? string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase));

            if (existingItem is null)
            {
                await core.Set<CommonObject>()
                    .AddAsync(entity: item, cancellationToken: cancellationToken);

                continue;
            }

            existingItem.Description = item.Description;
            existingItem.LastUpdated = item.LastUpdated;
            existingItem.LastUpdatedBy = item.LastUpdatedBy;
            existingItem.CreatedOn = item.CreatedOn;
            existingItem.CreatedBy = item.CreatedBy;
            existingItem.Version = item.Version;
            existingItem.Key = item.Key;
            existingItem.Type = item.Type;
            existingItem.Json = item.Json;
            existingItem.Culture = item.Culture;
        }

        await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    private static bool ContainsType(IEnumerable<string> itemTypes, string expectedType) =>
        itemTypes.Any(predicate: type => string.Equals(a: type, b: expectedType, comparisonType: StringComparison.OrdinalIgnoreCase));

    private static int GetPageDepth(string path) =>
        string.IsNullOrWhiteSpace(value: path)
            ? 0
            : path.Trim(trimChar: '/')
            .Split(separator: '/', options: StringSplitOptions.RemoveEmptyEntries).Length;

    private static string GetParentPagePath(string path)
    {
        string normalizedPath = NormalizePagePath(path: path);
        int separatorIndex = normalizedPath.LastIndexOf(value: '/');
        return separatorIndex <= 0 ? string.Empty : normalizedPath[..separatorIndex];
    }

    private static IEnumerable<string> ExtractFolderRolePaths(string data)
    {
        JToken token = JToken.Parse(json: data);

        IEnumerable<JObject> roles = token is JArray array
            ? array.OfType<JObject>()
            : token is JObject singleRole
                ? [singleRole]
                : [];

        foreach (JObject role in roles)
        {
            string path = role.Value<string>(key: "Path")?.Trim()
                .Trim(trimChar: '/') ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(value: path))
            {
                yield return path;
            }
        }
    }

    private static IEnumerable<(string Path, bool ShowOnMenus)> ExtractPageVisibility(string data)
    {
        JToken token = JToken.Parse(json: data);

        IEnumerable<JObject> pages = token is JArray array
            ? array.OfType<JObject>()
            : token is JObject singlePage
                ? [singlePage]
                : [];

        foreach (JObject page in pages)
        {
            string path = NormalizePagePath(path: page.Value<string>(key: "Path"));
            bool showOnMenus = page.Value<bool?>(key: "ShowOnMenus") ?? false;

            yield return (path, showOnMenus);
        }
    }

    private static string NormalizePagePath(string path) =>
        (path ?? string.Empty).Trim()
            .TrimStart(trimChar: '/');

    private static string GetParentFolderPath(string path)
    {
        int separatorIndex = path.LastIndexOf(value: '/');
        return separatorIndex <= 0 ? string.Empty : path[..separatorIndex];
    }

    private static string GetFolderName(string path)
    {
        int separatorIndex = path.LastIndexOf(value: '/');
        return separatorIndex < 0 ? path : path[(separatorIndex + 1)..];
    }

    private static string GetSizeOf(byte[] content)
    {
        if (content.Length > 1_000_000_000)
        {
            return $"{content.Length / 1000 / 1000 / 1000} GB";
        }

        if (content.Length > 1_000_000)
        {
            return $"{content.Length / 1000 / 1000} MB";
        }

        return content.Length > 1000
            ? $"{content.Length / 1000} KB"
            : $"{content.Length} B";
    }

    private static void NormalizeCommonObjects(IEnumerable<CommonObject> commonObjects, string createdBy)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (CommonObject commonObject in commonObjects)
        {
            NormalizeDateTimeOffsetProperty(commonObject: commonObject, propertyName: nameof(CommonObject.CreatedOn), fallbackValue: now);
            NormalizeDateTimeOffsetProperty(commonObject: commonObject, propertyName: nameof(CommonObject.LastUpdated), fallbackValue: now);
            NormalizeStringProperty(commonObject: commonObject, propertyName: nameof(CommonObject.CreatedBy), fallbackValue: createdBy);
            NormalizeStringProperty(commonObject: commonObject, propertyName: nameof(CommonObject.LastUpdatedBy), fallbackValue: createdBy);
            NormalizeCommonObjectJson(commonObject: commonObject, createdBy: createdBy, now: now);
        }
    }

    private static void NormalizeBaselinePackages(IEnumerable<Package> packages, string createdBy)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (Package package in packages)
        {
            foreach (PackageItem item in package.Items ?? [])
            {
                JToken data = JToken.Parse(json: item.Data);
                NormalizeAuditFields(token: data, createdBy: createdBy, now: now);
                item.Data = data.ToString(formatting: Formatting.None);
            }
        }
    }

    private static Package CreateAppImportPackage(Package package)
    {
        PackageItem[] items = (package.Items ?? [])
            .Select(selector: FilterPackageItemForAppImport)
            .Where(predicate: item => item is not null)
            .ToArray()!;

        return new Package
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = items,
        };
    }

    private static PackageItem FilterPackageItemForAppImport(PackageItem item)
    {
        if (CommonObjectOnlyTypes.Contains(item: item.Type))
        {
            return null;
        }

        if (!string.Equals(a: item.Type, b: "Core/Component", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return item;
        }

        JToken data = JToken.Parse(json: item.Data);
        JArray components = data is JArray array ? array : new JArray(data);

        JArray appScopedComponents = new(
            components
                .OfType<JObject>()
                .Where(predicate: component => AppScopedComponentNames.Contains(item: component.Value<string>(key: "Name") ?? string.Empty))
                .Select(selector: component => component.DeepClone()));

        if (appScopedComponents.Count == 0)
        {
            return null;
        }

        return new PackageItem
        {
            Id = item.Id,
            PackageId = item.PackageId,
            Type = item.Type,
            Data = appScopedComponents.ToString(formatting: Formatting.None),
        };
    }

    private static void NormalizeCommonObjectJson(CommonObject commonObject, string createdBy, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(value: commonObject.Json))
        {
            return;
        }

        JToken json = JToken.Parse(json: commonObject.Json);
        NormalizeAuditFields(token: json, createdBy: createdBy, now: now);
        commonObject.Json = json.ToString(formatting: Formatting.None);
    }

    private static void NormalizeAuditFields(JToken token, string createdBy, DateTimeOffset now)
    {
        foreach (JObject record in EnumerateObjects(token: token))
        {
            if (!LooksLikeBaselineRecord(record: record))
            {
                continue;
            }

            record["CreatedBy"] = createdBy;
            record["LastUpdatedBy"] = createdBy;
            record["CreatedOn"] = NormalizeDateValue(value: record["CreatedOn"], fallbackValue: now);
            record["LastUpdated"] = NormalizeDateValue(value: record["LastUpdated"], fallbackValue: now);
        }
    }

    private static IEnumerable<JObject> EnumerateObjects(JToken token)
    {
        if (token is JObject record)
        {
            yield return record;
        }

        if (token is not JContainer container)
        {
            yield break;
        }

        foreach (JObject descendant in container.Descendants()
            .OfType<JObject>())
        {
            yield return descendant;
        }
    }

    private static bool LooksLikeBaselineRecord(JObject record) =>
        record.ContainsKey(propertyName: "CreatedBy")
        || record.ContainsKey(propertyName: "LastUpdatedBy")
        || record.ContainsKey(propertyName: "CreatedOn")
        || record.ContainsKey(propertyName: "LastUpdated")
        || record.ContainsKey(propertyName: "Name")
        || record.ContainsKey(propertyName: "Html");

    private static JToken NormalizeDateValue(JToken value, DateTimeOffset fallbackValue) =>
        value is null
        || value.Type == JTokenType.Null
        || !DateTimeOffset.TryParse(input: value.ToString(), result: out DateTimeOffset parsed)
        || parsed == default
            ? fallbackValue
            : parsed;

    private static void NormalizeDateTimeOffsetProperty(CommonObject commonObject, string propertyName, DateTimeOffset fallbackValue)
    {
        PropertyInfo property = typeof(CommonObject).GetProperty(name: propertyName)!;
        object value = property.GetValue(obj: commonObject);

        if (value is null)
        {
            property.SetValue(obj: commonObject, value: fallbackValue);
            return;
        }

        if (value is DateTimeOffset dateTimeOffset && dateTimeOffset == default)
        {
            property.SetValue(obj: commonObject, value: fallbackValue);
        }
    }

    private static void NormalizeStringProperty(CommonObject commonObject, string propertyName, string fallbackValue)
    {
        PropertyInfo property = typeof(CommonObject).GetProperty(name: propertyName)!;

        property.SetValue(obj: commonObject, value: fallbackValue);
    }

    private static string BuildUserId(string email) =>
        (email ?? string.Empty)
            .Split(
                separator: '@',
                count: 2,
                options: StringSplitOptions.TrimEntries)[0]
            .Trim();
}