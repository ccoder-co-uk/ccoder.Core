// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreUser = cCoder.Data.Models.Security.User;
using SsoUser = cCoder.Security.Objects.Entities.SSOUser;

namespace cCoder.IntegrationTests.Infrastructure;

internal sealed class IntegrationAcceptanceSeeder(IServiceProvider services)
{
    private const int AppId = 1;
    private const string AppDomain = "localhost";
    private const string AcceptanceTenantId = "acceptance";
    private const string AcceptanceAdminRoleName = "Acceptance Administrators";
    private const string GuestUserId = "Guest";
    private const string AdminUserId = "admin";
    private const string AcceptanceAdminPrivileges =
        "app_admin,"
        + "app_create,app_read,app_update,app_delete,"
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
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        using DbContext sso = scope.ServiceProvider
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        await EnsureAppAsync(core: core);
        await EnsureCoreUserAsync(core: core,userId: GuestUserId,displayName: "Guest",email: string.Empty);
        await EnsureCoreUserAsync(core: core,userId: AdminUserId,displayName: "Acceptance Admin",email: "admin@localhost");
        await EnsureAcceptanceAdminRoleAsync(core: core);
        await EnsureUserHasRoleAsync(core: core,userId: GuestUserId);
        await EnsureUserHasRoleAsync(core: core,userId: AdminUserId);
        await EnsureTenantAsync(sso: sso);
        await EnsureSsoUserAsync(sso: sso,userId: GuestUserId,displayName: "Guest",email: string.Empty);
        await EnsureSsoUserAsync(sso: sso,userId: AdminUserId,displayName: "Acceptance Admin",email: "admin@localhost");
        await EnsureSsoAdministratorRoleAsync(sso: sso);
        await SeedCapturedAppDataAsync(core: core);
        await SeedCommonObjectsAsync(core: core);
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
                    TenantId = AcceptanceTenantId,
                    ConfigJson = AcceptanceAssetLoader.LoadText(fileName: "DefaultAppConfig.json"),
                });

            await core.SaveChangesAsync();
        }
    }

    private static async Task EnsureAcceptanceAdminRoleAsync(DbContext core)
    {
        Role role = await core.Set<Role>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(predicate: existing =>
            existing.AppId == AppId && existing.Name == AcceptanceAdminRoleName);

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
    }

    private static async Task EnsureUserHasRoleAsync(DbContext core, string userId)
    {
        Role role = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: existing =>
            existing.AppId == AppId && existing.Name == AcceptanceAdminRoleName);

        bool hasRole = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: existing =>
            existing.RoleId == role.Id && existing.UserId == userId);

        if (!hasRole)
        {
            core.Add(entity: new UserRole { RoleId = role.Id, UserId = userId });
            await core.SaveChangesAsync();
        }
    }

    private static async Task EnsureCoreUserAsync(DbContext core, string userId, string displayName, string email)
    {
        bool hasUser = await core.Set<CoreUser>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: existing => existing.Id == userId);

        if (!hasUser)
        {
            core.Add(
entity:                 new CoreUser
                {
                    Id = userId,
                    DefaultCultureId = string.Empty,
                    DisplayName = displayName,
                    Email = email,
                    IsActive = true,
                });

            await core.SaveChangesAsync();
        }
    }

    private static async Task EnsureSsoUserAsync(DbContext sso, string userId, string displayName, string email)
    {
        bool hasUser = await sso.Set<SsoUser>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: existing => existing.Id == userId);

        if (!hasUser)
        {
            sso.Add(entity: new SsoUser
            {
                Id = userId,
                DisplayName = displayName,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PhoneNumberConfirmed = false,
            });

            await sso.SaveChangesAsync();
        }
    }

    private static async Task EnsureTenantAsync(DbContext sso)
    {
        bool hasTenant = await sso.Set<Tenant>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: existing => existing.Id == AcceptanceTenantId);

        if (hasTenant)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        sso.Add(entity: new Tenant
        {
            Id = AcceptanceTenantId,
            Name = "Acceptance",
            Description = "Acceptance test tenant",
            CreatedBy = AdminUserId,
            LastUpdatedBy = AdminUserId,
            CreatedOn = now,
            LastUpdated = now
        });

        await sso.SaveChangesAsync();
    }

    private static async Task EnsureSsoAdministratorRoleAsync(DbContext sso)
    {
        SSORole role = await sso.Set<SSORole>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: existing =>
                existing.TenantId == AcceptanceTenantId
                && existing.Name == "Administrators");

        if (role is null)
        {
            role = new SSORole
            {
                Id = Guid.NewGuid(),
                TenantId = AcceptanceTenantId,
                Name = "Administrators",
                Description = "Acceptance tenant administrators",
                UsersArePortalAdmins = true,
                Privs = "security_admin,tenant_read,userrole_read,userrole_create,userrole_delete"
            };

            await sso.Set<SSORole>()
                .AddAsync(entity: role);

            await sso.SaveChangesAsync();
        }
        else if (!role.UsersArePortalAdmins)
        {
            role.UsersArePortalAdmins = true;
            await sso.SaveChangesAsync();
        }

        bool hasAdminRole = await sso.Set<SSOUserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: existing =>
                existing.RoleId == role.Id
                && existing.UserId == AdminUserId);

        if (!hasAdminRole)
        {
            await sso.Set<SSOUserRole>()
                .AddAsync(entity: new SSOUserRole
            {
                RoleId = role.Id,
                UserId = AdminUserId
            });

            await sso.SaveChangesAsync();
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

        CommonObject[] commonObjects = AcceptanceSeedData
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
            })
            .ToArray();

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

    private static void NormalizeDateTimeOffsetProperty(
        CommonObject commonObject,
        string propertyName,
        DateTimeOffset fallbackValue)
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

    private static async Task SeedRolesAsync(DbContext core)
    {
        string[] existingRoleNames = await core.Set<Role>()
            .Where(predicate: role => role.AppId == AppId)
            .Select(selector: role => role.Name)
            .ToArrayAsync();

        Role[] roles = AcceptanceSeedData
            .LoadRoles(packageName: "Roles",itemType: "Core/Role")
            .Where(predicate: role => !existingRoleNames.Contains(value: role.Name))
            .Select(selector: role => new Role
            {
                Id = Guid.NewGuid(),
                AppId = AppId,
                Name = role.Name,
                Description = role.Description,
                Privs = NormalizeRolePrivileges(role: role),
            })
            .ToArray();

        if (roles.Length == 0)
        {
            return;
        }

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

        Layout[] layouts = AcceptanceSeedData
            .LoadLayouts(packageName: "Layouts",itemType: "Core/Layout")
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
            })
            .ToArray();

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

        Template[] templates = AcceptanceSeedData
            .LoadTemplates(packageName: "Templates",itemType: "Core/Template")
            .Select(selector: template => new Template
            {
                Id = 0,
                AppId = AppId,
                Name = template.Name,
                Description = template.Description,
                ResourceKey = template.ResourceKey,
                RawString = NormalizeTemplate(template: template),
                CreatedOn = template.CreatedOn,
                CreatedBy = template.CreatedBy,
                LastUpdated = template.LastUpdated,
                LastUpdatedBy = template.LastUpdatedBy,
            })
            .ToArray();

        await core.Set<Template>()
            .AddRangeAsync(entities: templates);

        await core.SaveChangesAsync();
    }

    private static string NormalizeTemplate(Template template)
    {
        if (!string.Equals(a: template.Name,b: "UserInvite",comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return template.RawString;
        }

        return """
        <html style="font-family: [theme[font.family]]; width:800px; margin:0 auto; padding:0;">
            <head>
                <title>[email[subject]]</title>
                <style>
                    * { font-size: [theme[font.size]]; font-family: [theme[font.family]]; color: #1F2933; }
                    a { color: [theme[colours.links]]; cursor: pointer; }
                    hr { border-top: [theme[border.style]]; }
                </style>
            </head>
            <body style="width: 800px; margin: 20px auto; padding: 0; background: white;">
                <header style="padding: 20px 30px 0;">
                    <a href="[app[root]]" style="font-size: 28px; font-weight: 700; text-decoration: none; color: [theme[colours.primary]];">cCoder</a>
                    <h2 style="background: [theme[colours.primary]]; color: [theme[colours.text2]]; padding: 12px 16px; font-size: 140%; margin-top: 16px;">You have been invited</h2>
                </header>
                <div style="margin: 10px auto; padding: 5px 40px 30px;">
                    <p>[resource_description[InvitationStatement]]</p>
                    <p>[resource_displayname[Click]] <a href="[app[root]]/AcceptInvite?user=[model[SSOUser.Id]]&e=[model[CoreUser.Email]]&t=[model[EncodedToken]]">[resource_displayname[Here]]</a> to complete your account setup and sign in.</p>
                </div>
                <div style="background-color: [theme[colours.primary]]; color: [theme[colours.text2]]; width: 100%;">
                    <p style="padding: 10px; text-align: right; background: [theme[colours.primary]]; color: [theme[colours.text2]]; margin: 0;">&copy; 2026, cCoder</p>
                </div>
            </body>
        </html>
        """;
    }

    private static async Task SeedResourcesAsync(DbContext core)
    {
        if (await core.Set<Resource>()
            .AnyAsync(predicate: resource => resource.AppId == AppId))
        {
            return;
        }

        Resource[] resources = AcceptanceSeedData
            .LoadResources(packageName: "Resources",itemType: "Core/Resource")
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
            })
            .ToArray();

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

        Component[] components = AcceptanceSeedData
            .LoadComponents(packageName: "Components",itemType: "Core/Component")
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
            })
            .ToArray();

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

        Script[] scripts = AcceptanceSeedData
            .LoadScripts(packageName: "Scripts",itemType: "Core/Script")
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
            })
            .ToArray();

        await core.Set<Script>()
            .AddRangeAsync(entities: scripts);

        await core.SaveChangesAsync();
    }
}