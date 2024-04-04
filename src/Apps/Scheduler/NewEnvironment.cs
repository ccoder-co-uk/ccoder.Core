using cCoder.Core.Objects;
using cCoder.Core.Objects.Cryptos;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using log4net;
using Newtonsoft.Json;
using Security.Objects.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Scheduler
{
    public sealed class NewEnvironment : IDisposable
    {
        readonly ILog log = LogManager.GetLogger(typeof(NewEnvironment));

        Config Config { get; }
        ICoreDataContext CoreDb { get; }

        public NewEnvironment(Config config, ICoreDataContext core)
        {
            Config = config;
            CoreDb = core;
            core.DisableFilters();
        }

        public async Task Go(string rootDomain, string sourceDomain, string user, string pass)
        {
            if (CoreDb.GetAll<App>().Any())
                throw new InvalidOperationException("System is already setup!");

            try
            {
                // create the root admin account and import it in to all the databases
                User newAdmin = CreatePortalAdminUser(user, pass);

                // create the new empty root app
                App destinationApp = await CreateRootApp(rootDomain.Split(':')[0], newAdmin);
                newAdmin.Roles.First().Role.AppId = destinationApp.Id;

                // authenticate with our new credentials and post the packages we just pulled from the source
                using HttpClient destinationApi = new() { BaseAddress = new Uri($"https://{rootDomain}/Api/") };
                _ = await destinationApi.Authenticate(user, pass);

                // migrate 
                if (sourceDomain != null)
                {
                    // grab the destination packages for our new root app from the source domain
                    using HttpClient sourceApi = new() { BaseAddress = new Uri($"https://{sourceDomain}/Api/") };
                    _ = await sourceApi.Authenticate(user, pass);
                    await ImportFromSource(sourceApi, destinationApi, destinationApp, sourceDomain);
                }
                else
                    await ImportFromLocalData(destinationApi, destinationApp);

                _ = await destinationApi.GetAsync("RefreshCache");
            }
            catch (Exception ex)
            {
                log.Error("Setup failed due to an exception.\n", ex);
                throw;
            }
        }

        static async Task ImportFromSource(HttpClient from, HttpClient to, App app, string sourceDomain)
        {
            // Fetch Common Cache
            ODataCollection<CommonObject> cachedComponents = await from.GetAsync<ODataCollection<CommonObject>>($"cCoder.Core/CommonObject/Latest()?type=cCoder.Core/Component");
            ODataCollection<CommonObject> cachedResources = await from.GetAsync<ODataCollection<CommonObject>>($"cCoder.Core/CommonObject/Latest()?type=cCoder.Core/Resource");
            ODataCollection<CommonObject> cachedScripts = await from.GetAsync<ODataCollection<CommonObject>>($"cCoder.Core/CommonObject/Latest()?type=cCoder.Core/Script");
            ODataCollection<CommonObject> fullCache = new() { Value = cachedComponents.Value.Union(cachedResources.Value).Union(cachedScripts.Value).ToList() };

            // Fetch DMS 
            HttpResponseMessage folder = await from.GetAsync($"DMS/Content");

            // Fetch source packages 
            App sourceApp = (await from.GetAsync<ODataCollection<App>>($"cCoder.Core/App?$filter=Domain eq '{sourceDomain}'")).Value.First();
            ODataCollection<Package> packages = (await from.GetAsync<ODataCollection<Package>>("cCoder.Core/App(" + sourceApp.Id + ")/Export()?$expand=Items"));

            // and import it all
            await Import(to, app, packages, fullCache, folder.Content.ReadAsStream());
        }

        async Task ImportFromLocalData(HttpClient to, App app)
        {
            string basePath = Config.Settings["localDatafolder"];
            // Fetch Common Cache
            ODataCollection<CommonObject> cachedComponents = Data.ParseJson<ODataCollection<CommonObject>>(new StreamReader(File.OpenRead(basePath + "CommonComponents.json")).ReadToEnd());
            ODataCollection<CommonObject> cachedResources = Data.ParseJson<ODataCollection<CommonObject>>(new StreamReader(File.OpenRead(basePath + "CommonResources.json")).ReadToEnd());
            ODataCollection<CommonObject> cachedScripts = Data.ParseJson<ODataCollection<CommonObject>>(new StreamReader(File.OpenRead(basePath + "CommonScripts.json")).ReadToEnd());
            ODataCollection<CommonObject> fullCache = new() { Value = cachedComponents.Value.Union(cachedResources.Value).Union(cachedScripts.Value).ToList() };

            // Fetch source packages
            ODataCollection<Package> packages = Data.ParseJson<ODataCollection<Package>>(new StreamReader(File.OpenRead(basePath + "Packages.json")).ReadToEnd());
            await Import(to, app, packages, fullCache, File.OpenRead(basePath + "Content.zip"));
        }

        static async Task Import(HttpClient to, App app, ODataCollection<Package> packages, ODataCollection<CommonObject> commonCache, Stream fileData)
        {
            // import the DMS file data
            _ = await to.PostAsync($"DMS?unpack=true", new StreamContent(fileData));

            // import all the packages
            foreach (Package p in packages.Value)
                _ = await to.PostAsJsonAsync($"cCoder.Core/Package/ImportThis?appId={app.Id}", p);

            // import the common cache
            _ = await to.PostAsJsonAsync($"cCoder.Core/CommonObject/Import", commonCache);
        }

        User CreatePortalAdminUser(string user, string pass)
        {
            // add to members
            SSOUser rootUser = new()
            {
                Id = user,
                DisplayName = "Root Admin",
                Email = "root@localhost",
                PasswordHash = new AesCrypto<string>(Config.Settings["DecryptionKey"]).Encrypt(pass),
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            // Create the core user and return for the
            return new User
            {
                Id = rootUser.Id,
                IsActive = true,
                DisplayName = rootUser.DisplayName,
                Email = rootUser.Email,
                DefaultCultureId = string.Empty,
                Roles = new List<UserRole>
                {
                    new UserRole
                    {
                        UserId = user,
                        Role = new Role
                        {
                            Name = "Root Admins",
                            AppId = CoreDb.GetAll<App>().FirstOrDefault()?.Id ?? 0,
                            Privileges = CoreDb.GetAll<Privilege>().Select(p => p.Id).ToArray()
                        }
                    }
                }
            };
        }

        async Task<App> CreateRootApp(string domain, User admin)
        {
            App app = new()
            {
                DefaultCultureId = string.Empty,
                DefaultTheme = "Default",
                Domain = domain,
                Name = "Environment Admin",
                Cultures = CoreDb.GetAll<Culture>(true)
                    .Select(c => new AppCulture { CultureId = c.Id })
                    .ToList(),

                // build default config
                ConfigJson = JsonConvert.SerializeObject(DefaultAppConfig(), Formatting.Indented)
            };

            app = await CoreDb.AddAsync(app);
            List<Role> roles = new()
            {
                // portal admins role
                admin.Roles.First().Role,

                // standard app roles
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Administrators",
                    Privileges = CoreDb.GetAll<Privilege>().Where(p => !p.PortalAdminsOnly).Select(p => p.Id).ToList()
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Users",
                    Privileges = new List<string> { "culture_read,folderrole_read,pagerole_read,userrole_read,appculture_read,page_read,folder_read,file_read,app_read" }
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Guests",
                    Privileges = new List<string> { "folderrole_read,pagerole_read,userrole_read,appculture_read,page_read,folder_read,file_read,app_read" }
                }
            };

            roles.ForEach(r => r.AppId = app.Id);
            roles = (await CoreDb.AddAllAsync(roles)).ToList();
            admin.Roles = null;

            _ = await CoreDb.AddAllAsync(new[]
            {
                admin,
                new User { Id = "Guest", DisplayName = "Guest", Email = "guest@localhost", DefaultCultureId = string.Empty, IsActive = true }
            });

            _ = await CoreDb.AddAllAsync(new[] {
                new UserRole { UserId = admin.Id, RoleId = roles[0].Id },
                new UserRole { UserId = admin.Id, RoleId = roles[1].Id },
                new UserRole { UserId = "Guest", RoleId = roles[3].Id }
            });

            return app;
        }

        static object DefaultAppConfig() => new
        {
            Themes = new
            {
                Default = new
                {
                    margins = "4px",
                    paintLoginMid = false,
                    paintLoginBottom = false,
                    colours = new
                    {
                        primary = "#2e4b75",
                        secondary = "#EE8A00",
                        background = "white",
                        text = "#222",
                        text2 = "#FFFFFF",
                        links = "#214A71",
                        error = "red",
                        charts = new[] { "#FF1A0C", "#484848", "#484848", "#FF1A0C", "#484848", "#FF1A0C" },
                        margins = "8px"
                    },
                    font = new { size = "11px", family = "Quicksand, sans-serif" },
                    border = new { style = "solid 1px #ccc", width = "1px", radius = 0 },
                    notifications = new
                    {
                        error = new { text = "#222", background = "#FFECEC" },
                        warning = new { text = "#222", background = "#FFF4D9" },
                        info = new { text = "#222", background = "#E5F5FA" },
                        success = new { text = "#222", background = "#EAF7EC" }
                    },
                    shadows = "2px 2px 5px #333"
                }
            },
            Deployment = new
            {
                Targets = Array.Empty<object>(),
                DMS = new[] { "Content" }
            }
        };

        bool isDisposed = false;

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                CoreDb.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
