using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Security;
using static cCoder.Core.Services.Packaging.Importers.PageRoleImporter;

namespace cCoder.Core.Services.CMS;

public class AppService : CoreService<App>, IAppService
{
    private readonly IEnumerable<IPackageInstaller> importers;
    private readonly Config config;

    public AppService(IEnumerable<IPackageInstaller> importers, ICoreDataContext db, Config config) : base(db)
    {
        this.importers = importers;
        this.config = config;
    }

    public override async Task<App> AddAsync(App inputApp)
    {
        if (User.Can(null, "app_create"))
        {
            App newApp = new App().UpdateFrom(inputApp, true);

            if (string.IsNullOrEmpty(newApp.DefaultTheme))
                newApp.DefaultTheme = "Default"; // set theme of new apps to default

            App result = await base.AddAsync(newApp);

            await SetupCulturesForApp(newApp, result);

            await SetupRolesForApp(newApp);

            result = await Db.GetAll<App>(false)
                .Include(a => a.Roles)
                .FirstAsync(a => a.Id == result.Id);

            return result;
        }
        else
            throw new SecurityException("Access Denied!");
    }

    private async Task SetupCulturesForApp(App newApp, App result)
    {
        IEnumerable<string> cultureIds = newApp.Cultures?.Select(c => c.CultureId) ?? Array.Empty<string>();

        IEnumerable<AppCulture> appCultures = Db.GetAll<Culture>(false)
            .Where(c => c.Id == string.Empty || cultureIds.Contains(c.Id))
            .Select(c => new AppCulture { App = result, CultureId = c.Id });

        await Db.AddAllAsync(appCultures);

        if (string.IsNullOrEmpty(newApp.DefaultCultureId))
            newApp.DefaultCultureId = cultureIds.FirstOrDefault() ?? string.Empty; // set default culture to first or empty
    }

    private async Task SetupRolesForApp(App result)
    {
        User guest = await Db.GetAll<User>(false)
            .FirstOrDefaultAsync(u => u.Id == "Guest") 
                ?? 
               new User { Id = "Guest", DisplayName = "Guest", DefaultCultureId = string.Empty, IsActive = true, Email = "guest@corporatelinx.com" };

        Role[] roles = (await Db.AddAllAsync(
        [
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Administrators",
                App = result,
                Privileges = Db.GetAllPrivileges()
                    .Where(p => !p.PortalAdminsOnly)
                    .Select(p => p.Id)
                    .ToArray()
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Users",
                App = result,
                Privileges = ["culture_read,folderrole_read,pagerole_read,userrole_read,appculture_read,page_read,folder_read,file_read,app_read"]
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Guests",
                App = result,
                Privileges = ["folderrole_read,pagerole_read,userrole_read,appculture_read,page_read,folder_read,file_read,app_read"]
            }
        ])).ToArray();

        await Db.AddAllAsync(
            [
                new UserRole { RoleId = roles[0].Id, UserId = User.Id },
                new UserRole { RoleId = roles[1].Id, UserId = User.Id },
                new UserRole { RoleId = roles[2].Id, UserId = guest.Id }
            ]);
    }

    public override async Task<App> UpdateAsync(App app)
    {
        App dbVersion = await Db.GetAll<App>(true)
            .Include(a => a.Cultures)
            .FirstOrDefaultAsync(a => app.Id == a.Id);

        if (User.IsAdminOfApp(dbVersion.Id))
        {
            _ = dbVersion.UpdateFrom(app, true);
            dbVersion.Cultures = app.Cultures ?? dbVersion.Cultures;

            _ = await Db.UpdateAsync(dbVersion);
        }
        else
            throw new SecurityException("Access Denied!");

        return app;
    }

    public override Task DeleteAsync(object id)
    {
        if (User.IsAdminOfApp((int)id))
            Db.DeleteApp((int)id);
        else
            throw new SecurityException("Access Denied!");

        return Task.FromResult(0);
    }

    public IQueryable<User> GetAppUsers(int appId)
    {
        App app = Get(appId);
        return app == null
            ? throw new SecurityException("Access Denied!")
            : app.Roles.SelectMany(r => r.Users.Select(ru => ru.User)).AsQueryable();
    }

    public bool IsAdmin(int appId, string userName)
    {
        User user = Db.GetAll<User>()
            .Include(u => u.Roles)
            .FirstOrDefault(u => u.Id == userName);

        return Db.GetAll<App>()
            .Include(a => a.Roles.Select(r => r.Users))
            .FirstOrDefault(a => a.Id == appId)?
            .IsAppAdmin(user)
                ??
            false;
    }

    public async Task UpdatePageOrder(int key, App app)
    {
        // go get the app from the database
        App dbApp = Db.GetAll<App>(true)
            .Include("Pages")
            .FirstOrDefault(a => a.Id == key);

        if (dbApp != null)
        {
            // for each page in the app
            dbApp.Pages.ForEach(p =>
            {
                // find corresponding app from sent dataset
                Page sentPage = app.Pages.FirstOrDefault(pg => pg.Id == p.Id);
                if (sentPage != null)
                {
                    // update parent and order correspondingly
                    p.Order = sentPage.Order;
                    p.ParentId = sentPage.ParentId;
                }
            });

            // save changes made
            _ = await Db.SaveChangesAsync();
        }
        else
            throw new TaskCanceledException("App not found");
    }

    /// <summary>
    /// Exports an app for use with Import 
    /// </summary>
    /// <param name="appId">the app id of the app to export</param>
    /// <param name="packages">The list of package names to export</param>
    /// <returns>A stream which contains the app serialized in to a zip archive</returns>
    public IEnumerable<Package> Export(int appId, string[] packages = null)
    {
        if (!(User.Can(appId, "app_create") && User.Can(appId, "app_update") && User.Can(appId, "app_read") && User.Can(appId, "app_delete")))
            throw new SecurityException("Access Denied!");

        if (packages == null || packages.Length == 0)
            packages = 
            [
                "Roles",
                "Layouts",
                "Templates",
                "Resources",
                "Pages",
                "Workflows",
                "Components",
                "Scripts",
                "PageRoles",
                "FolderRoles",
                "Calendars",
                "CalendarEvents"
            ];

        Role[] roleData = Db.GetAll<Role>()
            .Where(r => r.AppId == appId)
            .Include(r => r.Users)
                .ThenInclude(ur => ur.User)
            .ToArray();

        Folder[] folderData = Db.GetAll<Folder>()
            .Where(r => r.AppId == appId)
            .Include(r => r.Roles)
            .ThenInclude(ur => ur.Role)
            .ToArray();

        List<PageRoleInfo> pageRoles = new();

        JsonSerializerSettings serializerSettings = ObjectExtensions.GetJSONSettings();
        serializerSettings.TypeNameHandling = TypeNameHandling.None;

        List<Package> result = new();

        packages.ForEach(pn => result.Add(pn switch
        {
            "Roles" => ExportRoles(roleData, serializerSettings),
            "FolderRoles" => ExportFolderRoles(folderData, serializerSettings),
            "Layouts" => ExportLayouts(appId, serializerSettings),
            "Templates" => ExportTemplates(appId, serializerSettings),
            "Components" => ExportComponents(appId, serializerSettings),
            "Scripts" => ExportScripts(appId, serializerSettings),
            "Resources" => ExportResources(appId, serializerSettings),
            "Pages" => ExportPages(appId, roleData, pageRoles, serializerSettings),
            "Workflows" => ExportWorkflows(appId, serializerSettings),
            "PageRoles" => ExportPageRoles(pageRoles, serializerSettings),
            "Calendars" => ExportCalendars(appId, serializerSettings),
            "CalendarEvents" => ExportCalendarEvents(appId, serializerSettings),
            _ => new Package(pn) { Items = Array.Empty<PackageItem>() }
        }));

        // some not needed values but required to make the packages valid before return to a client.
        string sourceApi = $"https://{Db.GetAll<App>(false).First(a => a.Id == appId).Domain}:{config.Settings["sslPort"]}/Api/";
        result.ForEach(p => { p.Description = "Generated by App export."; p.Category = "Dynamic"; p.SourceApi = sourceApi; });
        return result;
    }

    private static Package ExportPageRoles(List<PageRoleInfo> pageRoles, JsonSerializerSettings serializerSettings) => new("PageRoles")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/PageRole",
                    Data = pageRoles.ToJson(serializerSettings)
                }
            ]
    };

    private Package ExportWorkflows(int appId, JsonSerializerSettings serializerSettings) => new("Workflows")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/FlowDefinition",
                    Data = Db.GetAll<FlowDefinition>()
                        .Include(f => f.App)
                        .Where(f => f.AppId == appId)
                        .Select(f => new { ProcessName = f.App.Name, f.Name, f.ReportingComponentName, f.InstanceReportingComponentName, f.Description, f.DefinitionJson, f.ConfigJson, f.LastUpdated })
                        .ToJson(serializerSettings)
                }
            ]
    };

    private Package ExportCalendarEvents(int appId, JsonSerializerSettings serializerSettings) => new("CalendarEvents")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/CalendarEvent",
                    Data = Db.GetAll<CalendarEvent>()
                        .Include(f => f.Calendar)
                        .Where(f => f.Calendar.AppId == appId)
                        .Select(f => new { CalendarName = f.Calendar.Name, f.Name, f.Start, f.Description, f.DurationInTicks})
                        .ToJson(serializerSettings)
                }
            ]
    };

    private Package ExportCalendars(int appId, JsonSerializerSettings serializerSettings) => new("Calendars")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/Calendar",
                    Data = Db.GetAll<Calendar>()
                        .Where(p => p.AppId == appId)
                        .Select(p => new { p.Name, p.Description})
                        .ToJson(serializerSettings)
                }
            ]
    };

    private Package ExportPages(int appId, Role[] roleData, List<PageRoleInfo> pageRoles, JsonSerializerSettings serializerSettings)
    {
        var appPages = Db.GetAll<Page>()
            .Where(r => r.AppId == appId)
            .Include(p => p.Contents)
            .Include(p => p.PageInfo)
            .Include(p => p.Roles)
            .AsNoTracking()
            .ToList();

        var pageDict = appPages.ToDictionary(p => p.Id);

        foreach (var page in appPages)
            if (page.ParentId is not null && pageDict.TryGetValue(page.ParentId.Value, out var parent))
                page.Parent = parent;

        return new("Pages")
        {
            Items =
            [
                new PackageItem()
                {
                    Type = "Core/Page",
                    Data = appPages
                        .Select(p =>
                        {
                            if(p.Roles != null && p.Roles.Any())
                                pageRoles.AddRange(p.Roles.Select(r => new PageRoleInfo { Path = p.Path, Role = roleData.First(r2 => r2.Id == r.RoleId).Name }).ToArray());

                            var rootPage = p;
                            while(rootPage.ParentId is not null)
                                rootPage = rootPage.Parent;

                            if(p.ParentId is not null && string.IsNullOrEmpty(rootPage.Path))
                                p.Path = $"/{p.Path}";

                            return new
                            {
                                p.Path,
                                p.Name,
                                p.ResourceKey,
                                p.ShowOnMenus,
                                p.Order,
                                p.LastUpdated,
                                p.Layout,
                                Contents = p.Contents.Select(c => new { c.CultureId, c.Name, c.Html }).ToArray(),
                                PageInfo = p.PageInfo.Select(i => new { i.CultureId, i.Description, i.Keywords, i.Title }).ToArray()
                            };

                        })
                        .ToJson(serializerSettings)
                }
            ]
        };
    }

    private Package ExportResources(int appId, JsonSerializerSettings serializerSettings) => new("Resources")
    {
        Items =
            [
                new PackageItem() {
                    Type = "Core/Resource",
                    Data = Db.GetAll<Resource>()
                        .Where(r => r.AppId == appId)
                        .Select(r => new { r.Culture, r.Key, r.Name, r.DisplayName, r.ShortDisplayName, r.Description, r.LastUpdated })
                        .ToJson(serializerSettings)
                }
            ]
    };

    private Package ExportScripts(int appId, JsonSerializerSettings serializerSettings) => new("Scripts")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/Script",
                    Data = Db.GetAll<Script>()
                        .Where(r => r.AppId == appId)
                        .Select(c => new { c.Name, c.Content, c.LastUpdated })
                        .ToJson(serializerSettings)
                }
            ]
    };

    private Package ExportComponents(int appId, JsonSerializerSettings serializerSettings) => new("Components")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/Component",
                    Data = Db.GetAll<Component>()
                        .Where(r => r.AppId == appId)
                        .Select(c => new { c.Name, c.Key, c.ResourceKey, c.Script, c.Content, c.LastUpdated })
                        .ToJson(serializerSettings)
                }
            ]
    };

    private Package ExportTemplates(int appId, JsonSerializerSettings serializerSettings) => new("Templates")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/Template",
                    Data = Db.GetAll<Template>()
                        .Where(r => r.AppId == appId)
                        .Select(t => new { t.Name, t.ResourceKey, t.RawString, t.LastUpdated })
                        .ToJson(serializerSettings)
                }
            ]
    };

    private Package ExportLayouts(int appId, JsonSerializerSettings serializerSettings) => new("Layouts")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/Layout",
                    Data = Db.GetAll<Layout>()
                        .Where(r => r.AppId == appId)
                        .Select(l => new { l.Name, l.HeaderHtml, l.Html, l.Script, l.LastUpdated })
                        .ToJson(serializerSettings)
                }
            ]
    };

    private static Package ExportFolderRoles(Folder[] folderData, JsonSerializerSettings serializerSettings) => new("FolderRoles")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/FolderRole",
                    Data = folderData.SelectMany(f => f.Roles, (folder, role) => new { folder.Path, role.Role.Name })
                        .ToArray()
                        .ToJson(serializerSettings)
                }
            ]
    };

    private static Package ExportRoles(Role[] roleData, JsonSerializerSettings serializerSettings) => new("Roles")
    {
        Items =
            [
                new PackageItem()
                {
                    Type = "Core/Role",
                    Data = roleData.Select(r => new { r.Name, r.Privs }).ToJson(serializerSettings)
                }
            ]
    };

    /// <summary>
    /// Imports an archive produced by Export defined here
    /// </summary>
    /// <param name="name">The destination app name</param>
    /// <param name="domain">The destination app Domain</param>
    /// <param name="packageStream">The zip archive stream source for the import</param>
    /// <returns></returns>
    public async Task Import(string name, string domain, Stream packageStream)
    {
        // fetch or create the app
        App app = GetAll().FirstOrDefault(a => a.Domain.ToLower() == domain.ToLower())
            ??
        await AddAsync(new App() { Name = name, Domain = domain });

        // confirm user has admin rights 
        if (User.IsAdminOfApp(app.Id))
        {
            Package package = new() { Category = "Generated From Zip Source", Name = "App Import", Items = new List<PackageItem>() };

            using (ZipArchive z = new(packageStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry e in z.Entries.Where(e => e.Name.EndsWith(".json")))
                {
                    using StreamReader reader = new(e.Open());
                    package.Items.Add(new PackageItem
                    {
                        Type = e.FullName.TrimEnd("s.json".ToCharArray()),
                        Data = await reader.ReadToEndAsync()
                    });
                }
            }

            foreach (IPackageInstaller ctx in importers)
                await ctx.Import(app.Id, package);
        }
        else
            throw new SecurityException("Access Denied!");
    }

    #region Private Methods

    private IEnumerable<Page> PagesWithinTree(Page root) => root.Pages
        .SelectMany(p =>
        {
            List<Page> r = PagesWithinTree(p).ToList();
            r.Add(p);
            return r;
        })
        .Distinct();
    #endregion
}