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
        + "calendar_create,calendar_read,calendar_update,calendar_delete,"
        + "commonobject_create,commonobject_update,commonobject_delete,"
        + "culture_create,culture_update,culture_delete,"
        + "file_create,file_read,file_update,file_delete,"
        + "filecontent_create,filecontent_read,filecontent_update,filecontent_delete,"
        + "flowdefinition_create,flowdefinition_read,flowdefinition_update,flowdefinition_delete,flowdefinition_execute,"
        + "folder_create,folder_read,folder_update,folder_delete,"
        + "package_create,package_update,package_delete,"
        + "packageitem_create,packageitem_update,packageitem_delete,"
        + "scheduledtask_create,scheduledtask_read,scheduledtask_update,scheduledtask_delete,"
        + "workflowevent_create,workflowevent_read,workflowevent_update,workflowevent_delete";

    public async Task SeedAsync()
    {
        using IServiceScope scope = services.CreateScope();
        using DbContext core = scope.ServiceProvider
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();
        using DbContext sso = scope.ServiceProvider
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);

        await EnsureAppAsync(core);
        await EnsureCoreUserAsync(core, GuestUserId, "Guest", string.Empty);
        await EnsureCoreUserAsync(core, AdminUserId, "Acceptance Admin", "admin@localhost");
        await EnsureAcceptanceAdminRoleAsync(core);
        await EnsureUserHasRoleAsync(core, GuestUserId);
        await EnsureUserHasRoleAsync(core, AdminUserId);
        await EnsureSsoUserAsync(sso, GuestUserId, "Guest", string.Empty);
        await EnsureSsoUserAsync(sso, AdminUserId, "Acceptance Admin", "admin@localhost");
        await SeedCapturedAppDataAsync(core);
        await SeedCommonObjectsAsync(core);
    }

    private static async Task EnsureAppAsync(DbContext core)
    {
        bool hasApp = await core.Set<App>().AnyAsync(app => app.Id == AppId);

        if (!hasApp)
        {
            core.Add(
                new App
                {
                    Name = "Acceptance",
                    Domain = AppDomain,
                    DefaultTheme = "Default",
                    DefaultCultureId = string.Empty,
                    TenantId = AcceptanceTenantId,
                    ConfigJson = AcceptanceAssetLoader.LoadText("DefaultAppConfig.json"),
                });

            await core.SaveChangesAsync();
        }
    }

    private static async Task EnsureAcceptanceAdminRoleAsync(DbContext core)
    {
        Role role = await core.Set<Role>().IgnoreQueryFilters().FirstOrDefaultAsync(existing =>
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

            core.Add(role);
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
        Role role = await core.Set<Role>().IgnoreQueryFilters().SingleAsync(existing =>
            existing.AppId == AppId && existing.Name == AcceptanceAdminRoleName);

        bool hasRole = await core.Set<UserRole>().IgnoreQueryFilters().AnyAsync(existing =>
            existing.RoleId == role.Id && existing.UserId == userId);

        if (!hasRole)
        {
            core.Add(new UserRole { RoleId = role.Id, UserId = userId });
            await core.SaveChangesAsync();
        }
    }

    private static async Task EnsureCoreUserAsync(DbContext core, string userId, string displayName, string email)
    {
        bool hasUser = await core.Set<CoreUser>().IgnoreQueryFilters().AnyAsync(existing => existing.Id == userId);

        if (!hasUser)
        {
            core.Add(
                new CoreUser
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
        bool hasUser = await sso.Set<SsoUser>().IgnoreQueryFilters().AnyAsync(existing => existing.Id == userId);

        if (!hasUser)
        {
            sso.Add(new SsoUser
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

    private static async Task SeedCapturedAppDataAsync(DbContext core)
    {
        await SeedRolesAsync(core);
        await SeedLayoutsAsync(core);
        await SeedTemplatesAsync(core);
        await SeedResourcesAsync(core);
        await SeedComponentsAsync(core);
        await SeedScriptsAsync(core);
    }

    private static async Task SeedCommonObjectsAsync(DbContext core)
    {
        if (await core.Set<CommonObject>().AnyAsync())
            return;

        CommonObject[] commonObjects = AcceptanceSeedData
            .LoadCommonObjects()
            .Select(item => new CommonObject
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

        NormalizeCommonObjects(commonObjects);

        await core.Set<CommonObject>().AddRangeAsync(commonObjects);
        await core.SaveChangesAsync();
    }

    private static void NormalizeCommonObjects(IEnumerable<CommonObject> commonObjects)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (CommonObject commonObject in commonObjects)
        {
            NormalizeDateTimeOffsetProperty(commonObject, nameof(CommonObject.CreatedOn), now);
            NormalizeDateTimeOffsetProperty(commonObject, nameof(CommonObject.LastUpdated), now);
            NormalizeStringProperty(commonObject, nameof(CommonObject.CreatedBy), "acceptance");
            NormalizeStringProperty(commonObject, nameof(CommonObject.LastUpdatedBy), "acceptance");
        }
    }

    private static void NormalizeDateTimeOffsetProperty(
        CommonObject commonObject,
        string propertyName,
        DateTimeOffset fallbackValue)
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

    private static async Task SeedRolesAsync(DbContext core)
    {
        if (await core.Set<Role>().AnyAsync(role => role.AppId == AppId && role.Name != AcceptanceAdminRoleName))
            return;

        Role[] roles = AcceptanceSeedData
            .LoadPackageItems<Role>("Roles", "Core/Role")
            .Select(role => new Role
            {
                Id = Guid.NewGuid(),
                AppId = AppId,
                Name = role.Name,
                Description = role.Description,
                Privs = role.Privs,
            })
            .ToArray();

        await core.Set<Role>().AddRangeAsync(roles);
        await core.SaveChangesAsync();
    }

    private static async Task SeedLayoutsAsync(DbContext core)
    {
        if (await core.Set<Layout>().AnyAsync(layout => layout.AppId == AppId))
            return;

        Layout[] layouts = AcceptanceSeedData
            .LoadPackageItems<Layout>("Layouts", "Core/Layout")
            .Select(layout => new Layout
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

        await core.Set<Layout>().AddRangeAsync(layouts);
        await core.SaveChangesAsync();
    }

    private static async Task SeedTemplatesAsync(DbContext core)
    {
        if (await core.Set<Template>().AnyAsync(template => template.AppId == AppId))
            return;

        Template[] templates = AcceptanceSeedData
            .LoadPackageItems<Template>("Templates", "Core/Template")
            .Select(template => new Template
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
            })
            .ToArray();

        await core.Set<Template>().AddRangeAsync(templates);
        await core.SaveChangesAsync();
    }

    private static async Task SeedResourcesAsync(DbContext core)
    {
        if (await core.Set<Resource>().AnyAsync(resource => resource.AppId == AppId))
            return;

        Resource[] resources = AcceptanceSeedData
            .LoadPackageItems<Resource>("Resources", "Core/Resource")
            .Select(resource => new Resource
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

        await core.Set<Resource>().AddRangeAsync(resources);
        await core.SaveChangesAsync();
    }

    private static async Task SeedComponentsAsync(DbContext core)
    {
        if (await core.Set<Component>().AnyAsync(component => component.AppId == AppId))
            return;

        Component[] components = AcceptanceSeedData
            .LoadPackageItems<Component>("Components", "Core/Component")
            .Select(component => new Component
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

        await core.Set<Component>().AddRangeAsync(components);
        await core.SaveChangesAsync();
    }

    private static async Task SeedScriptsAsync(DbContext core)
    {
        if (await core.Set<Script>().AnyAsync(script => script.AppId == AppId))
            return;

        Script[] scripts = AcceptanceSeedData
            .LoadPackageItems<Script>("Scripts", "Core/Script")
            .Select(script => new Script
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

        await core.Set<Script>().AddRangeAsync(scripts);
        await core.SaveChangesAsync();
    }
}
