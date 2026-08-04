// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ContentMetadataCache = cCoder.ContentManagement.Exposures.Caching.IMetadataCache;


namespace Web.AcceptanceTests.Infrastructure;

internal sealed class AcceptanceApplicationSeeder(IServiceProvider services)
{
    internal const string MetadataAdministratorToken =
        "metadata-administrator-token";

    internal const string MetadataOrdinaryUserToken =
        "metadata-ordinary-user-token";

    private const int AppId = 1;
    private const string AppDomain = "localhost";
    private const string AcceptanceAdminRoleName = "Acceptance Administrators";
    private const string MetadataAdministratorUserId =
        "metadata.administrator";

    private const string MetadataOrdinaryUserId =
        "metadata.ordinary";
    private const string AcceptanceAdminPrivileges =
        "app_admin,"
        + "app_read,"
        + "appculture_read,"
        + "calendar_create,calendar_read,calendar_update,calendar_delete,"
        + "commonobject_create,commonobject_update,commonobject_delete,"
        + "culture_create,culture_read,culture_update,culture_delete,"
        + "file_create,file_read,file_update,file_delete,"
        + "filecontent_create,filecontent_read,filecontent_update,filecontent_delete,"
        + "flowdefinition_create,flowdefinition_read,flowdefinition_update,flowdefinition_delete,flowdefinition_execute,"
        + "folder_create,folder_read,folder_update,folder_delete,"
        + "folderrole_read,"
        + "package_create,package_update,package_delete,"
        + "packageitem_create,packageitem_update,packageitem_delete,"
        + "page_read,pagerole_read,"
        + "scheduledtask_create,scheduledtask_read,scheduledtask_update,scheduledtask_delete,"
        + "user_create,user_read,user_update,user_delete,"
        + "userrole_create,userrole_read,userrole_update,userrole_delete,"
        + "workflowevent_create,workflowevent_read,workflowevent_update,workflowevent_delete";

    public async Task SeedAsync()
    {
        using IServiceScope scope = services.CreateScope();

        using DbContext core = scope.ServiceProvider
            .GetRequiredService<cCoder.Data.ICoreContextFactory>()
            .CreateCoreContext();

        using DbContext security = scope.ServiceProvider
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        await EnsureAppAsync(core: core);
        await EnsureGuestUserAsync(core: core);

        await EnsureMetadataUsersAsync(
            core: core,
            security: security);

        await EnsureGuestAdminAsync(core: core);
        await SeedCapturedAppDataAsync(core: core);
        await SeedCommonObjectsAsync(core: core);
        RefreshCaches(services: scope.ServiceProvider);
    }

    private static async Task EnsureMetadataUsersAsync(
        DbContext core,
        DbContext security)
    {
        await EnsureSecurityUserAndTokenAsync(
            security: security,
            userId: MetadataAdministratorUserId,
            tokenId: MetadataAdministratorToken);

        await EnsureSecurityUserAndTokenAsync(
            security: security,
            userId: MetadataOrdinaryUserId,
            tokenId: MetadataOrdinaryUserToken);

        await EnsureMetadataSecurityRoleAsync(
            security: security);

        foreach (string userId in new[]
        {
            MetadataAdministratorUserId,
            MetadataOrdinaryUserId,
        })
        {
            bool hasUser = await core.Set<User>()
                .AnyAsync(predicate: user => user.Id == userId);

            if (!hasUser)
            {
                core.Add(entity: new User
                {
                    Id = userId,
                    DefaultCultureId = string.Empty,
                    DisplayName = userId,
                    Email = $"{userId}@example.test",
                    IsActive = true,
                });
            }
        }

        await core.SaveChangesAsync();
    }

    private static async Task EnsureMetadataSecurityRoleAsync(
        DbContext security)
    {
        const string tenantId = "acceptance";
        const string roleName = "Acceptance Tenant Admins";

        Tenant tenant = await security.Set<Tenant>()
            .FirstOrDefaultAsync(predicate: item =>
                item.Id == tenantId);

        if (tenant is null)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            tenant = new Tenant
            {
                Id = tenantId,
                Name = "Acceptance",
                Description = "Acceptance tenant",
                CreatedBy = MetadataAdministratorUserId,
                LastUpdatedBy = MetadataAdministratorUserId,
                CreatedOn = now,
                LastUpdated = now,
            };

            security.Add(entity: tenant);
            await security.SaveChangesAsync();
        }

        SSORole role = await security.Set<SSORole>()
            .FirstOrDefaultAsync(predicate: item =>
                item.TenantId == tenantId
                && item.Name == roleName);

        if (role is null)
        {
            role = new SSORole
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = roleName,
                Description = "Acceptance tenant administrators",
                Privs = "api_metadata_read,tenant_admin,tenant_read",
                UsersArePortalAdmins = false,
            };

            security.Add(entity: role);
            await security.SaveChangesAsync();
        }

        bool hasRole = await security.Set<SSOUserRole>()
            .AnyAsync(predicate: item =>
                item.RoleId == role.Id
                && item.UserId == MetadataAdministratorUserId);

        if (!hasRole)
        {
            security.Add(entity: new SSOUserRole
            {
                RoleId = role.Id,
                UserId = MetadataAdministratorUserId,
            });

            await security.SaveChangesAsync();
        }
    }

    private static async Task EnsureSecurityUserAndTokenAsync(
        DbContext security,
        string userId,
        string tokenId)
    {
        bool hasUser = await security.Set<SSOUser>()
            .AnyAsync(predicate: user => user.Id == userId);

        if (!hasUser)
        {
            security.Add(entity: new SSOUser
            {
                Id = userId,
                DisplayName = userId,
                Email = $"{userId}@example.test",
                EmailConfirmed = true,
            });
        }

        bool hasToken = await security.Set<Token>()
            .AnyAsync(predicate: token => token.Id == tokenId);

        if (!hasToken)
        {
            security.Add(entity: new Token
            {
                Id = tokenId,
                UserName = userId,
                Reason = (int)TokenUse.Auth,
                Expires = DateTimeOffset.UtcNow.AddHours(hours: 1),
            });
        }

        await security.SaveChangesAsync();
    }

    private static async Task EnsureAppAsync(DbContext core)
    {
        bool hasApp = await core.Set<App>()
            .AnyAsync(predicate: app => app.Id == AppId);

        if (!hasApp)
        {
            core.Add(
entity:                 new App
                {
                    Name = "Acceptance",
                    Domain = AppDomain,
                    DefaultTheme = "Default",
                    DefaultCultureId = string.Empty,
                    TenantId = "acceptance",
                    ConfigJson = AcceptanceAssetLoader.LoadText(fileName: "DefaultAppConfig.json"),
                }
            );

            await core.SaveChangesAsync();
        }
    }

    private static async Task EnsureGuestAdminAsync(DbContext core)
    {
        Role role = await core.Set<Role>()
            .FirstOrDefaultAsync(predicate: existing =>
            existing.AppId == AppId && existing.Name == AcceptanceAdminRoleName
        );

        if (role is null)
        {
            role = new Role
            {
                Id = Guid.NewGuid(),
                AppId = AppId,
                Name = AcceptanceAdminRoleName,
                Description = "Acceptance bootstrap role",
                Privs = AcceptanceAdminPrivileges,
            };

            core.Add(entity: role);
            await core.SaveChangesAsync();
        }
        else if (role.Privs != AcceptanceAdminPrivileges)
        {
            role.Privs = AcceptanceAdminPrivileges;
            await core.SaveChangesAsync();
        }

        bool hasGuestRole = await core.Set<UserRole>()
            .AnyAsync(predicate: existing =>
            existing.RoleId == role.Id && existing.UserId == "Guest"
        );

        if (!hasGuestRole)
        {
            core.Add(entity: new UserRole { RoleId = role.Id, UserId = "Guest" });
            await core.SaveChangesAsync();
        }

        bool hasMetadataAdministratorRole =
            await core.Set<UserRole>()
                .AnyAsync(predicate: existing =>
                    existing.RoleId == role.Id
                    && existing.UserId ==
                        MetadataAdministratorUserId);

        if (!hasMetadataAdministratorRole)
        {
            core.Add(entity: new UserRole
            {
                RoleId = role.Id,
                UserId = MetadataAdministratorUserId,
            });

            await core.SaveChangesAsync();
        }
    }

    private static async Task EnsureGuestUserAsync(DbContext core)
    {
        bool hasGuestUser = await core.Set<User>()
            .AnyAsync(predicate: existing => existing.Id == "Guest");

        if (!hasGuestUser)
        {
            core.Add(
entity:                 new User
                {
                    Id = "Guest",
                    DefaultCultureId = string.Empty,
                    DisplayName = "Guest",
                    Email = string.Empty,
                    IsActive = true,
                }
            );

            await core.SaveChangesAsync();
        }
    }

    private static async Task SeedCapturedAppDataAsync(DbContext core)
    {
        await SeedRolesAsync(core: core);
        await SeedLayoutsAsync(core: core);
        await SeedTemplatesAsync(core: core);
        await SeedResourcesAsync(core: core);
        await SeedComponentsAsync(core: core);
        await SeedScriptsAsync(core: core);
    }

    private static async Task SeedCommonObjectsAsync(DbContext core)
    {
        if (await core.Set<CommonObject>()
            .AnyAsync())
        {
            return;
        }

        CommonObject[] commonObjects = [.. AcceptanceSeedData
            .LoadCommonObjects()
            .Select(selector: item => new CommonObject
            {
                Id = 0,
                Name = item.Name,
                Description = item.Description,
                LastUpdated = item.LastUpdated,
                LastUpdatedBy = item.LastUpdatedBy,
                CreatedOn = item.CreatedOn,
                CreatedBy = item.CreatedBy,
                Version = item.Version,
                Key = item.Key,
                Type = item.Type,
                Json = item.Json,
                Culture = item.Culture,
            })];

        NormalizeCommonObjects(commonObjects: commonObjects);

        await core.Set<CommonObject>()
            .AddRangeAsync(entities: commonObjects);

        await core.SaveChangesAsync();
    }

    private static void NormalizeCommonObjects(IEnumerable<CommonObject> commonObjects)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (CommonObject commonObject in commonObjects)
        {
            NormalizeDateTimeOffsetProperty(commonObject: commonObject,propertyName: nameof(CommonObject.CreatedOn),fallbackValue: now);
            NormalizeDateTimeOffsetProperty(commonObject: commonObject,propertyName: nameof(CommonObject.LastUpdated),fallbackValue: now);
            NormalizeStringProperty(commonObject: commonObject,propertyName: nameof(CommonObject.CreatedBy),fallbackValue: "acceptance");
            NormalizeStringProperty(commonObject: commonObject,propertyName: nameof(CommonObject.LastUpdatedBy),fallbackValue: "acceptance");
        }
    }

    private static void NormalizeDateTimeOffsetProperty(CommonObject commonObject, string propertyName, DateTimeOffset fallbackValue)
    {
        PropertyInfo property = typeof(CommonObject).GetProperty(name: propertyName)!;
        object value = property.GetValue(obj: commonObject);

        if (value is null)
        {
            property.SetValue(obj: commonObject,value: fallbackValue);
            return;
        }

        if (value is DateTimeOffset dateTimeOffset && dateTimeOffset == default)
        {
            property.SetValue(obj: commonObject,value: fallbackValue);
        }
    }

    private static void NormalizeStringProperty(CommonObject commonObject, string propertyName, string fallbackValue)
    {
        PropertyInfo property = typeof(CommonObject).GetProperty(name: propertyName)!;

        if (property.GetValue(obj: commonObject) is not string value || string.IsNullOrWhiteSpace(value: value))
        {
            property.SetValue(obj: commonObject,value: fallbackValue);
        }
    }

    private static void RefreshCaches(IServiceProvider services)
    {
        cCoder.ContentManagement.Exposures.Caching.ICommonObjectCache commonObjectCache =
            services.GetRequiredService<cCoder.ContentManagement.Exposures.Caching.ICommonObjectCache>();

        ContentMetadataCache metadataCache = services.GetRequiredService<ContentMetadataCache>();

        commonObjectCache.Refresh();
        metadataCache.Rebuild();
    }

    private static async Task SeedRolesAsync(DbContext core)
    {
        if (await core.Set<Role>()
            .AnyAsync(predicate: role => role.AppId == AppId && role.Name != AcceptanceAdminRoleName))
        {
            return;
        }

        Role[] roles = [.. AcceptanceSeedData
            .LoadRoles(packageName: "Roles",itemType: "Core/Role")
            .Select(selector: role => new Role
            {
                Id = Guid.NewGuid(),
                AppId = AppId,
                Name = role.Name,
                Description = role.Description,
                Privs = NormalizeRolePrivileges(role: role),
            })];

        await core.Set<Role>()
            .AddRangeAsync(entities: roles);

        await core.SaveChangesAsync();
    }

    private static string NormalizeRolePrivileges(Role role)
    {
        if (role.Name != "Users" || role.Privs?.Split(separator: ',')
            .Contains(value: "user_update") == true)
        {
            return role.Privs;
        }

        return string.IsNullOrWhiteSpace(value: role.Privs)
            ? "user_update"
            : $"{role.Privs},user_update";
    }

    private static async Task SeedLayoutsAsync(DbContext core)
    {
        if (await core.Set<Layout>()
            .AnyAsync(predicate: layout => layout.AppId == AppId))
        {
            return;
        }

        Layout[] layouts = [.. AcceptanceSeedData
            .LoadLayouts(packageName: "Layouts",itemType: "ContentManagement/Layout")
            .Select(selector: layout => new Layout
            {
                Id = 0,
                AppId = AppId,
                Name = layout.Name,
                Description = layout.Description,
                HeaderHtml = layout.HeaderHtml,
                Html = layout.Html,
                Script = layout.Script,
                CreatedOn = layout.CreatedOn,
                CreatedBy = layout.CreatedBy,
                LastUpdated = layout.LastUpdated,
                LastUpdatedBy = layout.LastUpdatedBy,
            })];

        await core.Set<Layout>()
            .AddRangeAsync(entities: layouts);

        await core.SaveChangesAsync();
    }

    private static async Task SeedTemplatesAsync(DbContext core)
    {
        if (await core.Set<Template>()
            .AnyAsync(predicate: template => template.AppId == AppId))
        {
            return;
        }

        Template[] templates = [.. AcceptanceSeedData
            .LoadTemplates(packageName: "Templates",itemType: "ContentManagement/Template")
            .Select(selector: template => new Template
            {
                Id = 0,
                AppId = AppId,
                Name = template.Name,
                Description = template.Description,
                ResourceKey = template.ResourceKey,
                RawString = template.RawString,
                CreatedOn = template.CreatedOn,
                CreatedBy = template.CreatedBy,
                LastUpdated = template.LastUpdated,
                LastUpdatedBy = template.LastUpdatedBy,
            })];

        await core.Set<Template>()
            .AddRangeAsync(entities: templates);

        await core.SaveChangesAsync();
    }

    private static async Task SeedResourcesAsync(DbContext core)
    {
        if (await core.Set<Resource>()
            .AnyAsync(predicate: resource => resource.AppId == AppId))
        {
            return;
        }

        Resource[] resources = [.. AcceptanceSeedData
            .LoadResources(packageName: "Resources",itemType: "ContentManagement/Resource")
            .Select(selector: resource => new Resource
            {
                Id = 0,
                AppId = AppId,
                Name = resource.Name,
                Description = resource.Description,
                Key = resource.Key,
                Culture = resource.Culture ?? string.Empty,
                DisplayName = resource.DisplayName,
                ShortDisplayName = resource.ShortDisplayName,
                CreatedOn = resource.CreatedOn,
                CreatedBy = resource.CreatedBy,
                LastUpdated = resource.LastUpdated,
                LastUpdatedBy = resource.LastUpdatedBy,
            })];

        await core.Set<Resource>()
            .AddRangeAsync(entities: resources);

        await core.SaveChangesAsync();
    }

    private static async Task SeedComponentsAsync(DbContext core)
    {
        if (await core.Set<Component>()
            .AnyAsync(predicate: component => component.AppId == AppId))
        {
            return;
        }

        Component[] components = [.. AcceptanceSeedData
            .LoadComponents(packageName: "Components",itemType: "ContentManagement/Component")
            .Select(selector: component => new Component
            {
                Id = 0,
                AppId = AppId,
                Name = component.Name,
                Description = component.Description,
                ResourceKey = component.ResourceKey,
                Content = component.Content,
                Script = component.Script,
                Key = component.Key,
                CreatedOn = component.CreatedOn,
                CreatedBy = component.CreatedBy,
                LastUpdated = component.LastUpdated,
                LastUpdatedBy = component.LastUpdatedBy,
            })];

        await core.Set<Component>()
            .AddRangeAsync(entities: components);

        await core.SaveChangesAsync();
    }

    private static async Task SeedScriptsAsync(DbContext core)
    {
        if (await core.Set<Script>()
            .AnyAsync(predicate: script => script.AppId == AppId))
        {
            return;
        }

        Script[] scripts = [.. AcceptanceSeedData
            .LoadScripts(packageName: "Scripts",itemType: "ContentManagement/Script")
            .Select(selector: script => new Script
            {
                Id = 0,
                AppId = AppId,
                Name = script.Name,
                Description = script.Description,
                Key = script.Key,
                Content = script.Content,
                CreatedOn = script.CreatedOn,
                CreatedBy = script.CreatedBy,
                LastUpdated = script.LastUpdated,
                LastUpdatedBy = script.LastUpdatedBy,
            })];

        await core.Set<Script>()
            .AddRangeAsync(entities: scripts);

        await core.SaveChangesAsync();
    }
}