using cCoder.Core.Data;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Auth;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Testcontainers.MsSql;

namespace cCoder.Core.E2E.IntegrationTests
{
    public class ApplicationFixture : IAsyncLifetime
    {
#pragma warning disable CS8618
        WebApplicationFactory<Web.Program> factory;
        MsSqlContainer sqlContainer;
        public HttpClient Client;
        public CoreDataContext CoreDataContext { get; set; }

#pragma warning restore CS8618 // Seems to not understand IAsyncLifetime

        public async Task InitializeAsync()
        {
            sqlContainer = new MsSqlBuilder()
                .WithImage("ghcr.io/ccoder-co-uk/sql-core:main")
                .WithPortBinding(1433, assignRandomHostPort: true)
                .WithPassword("test123!!")
                .Build();

            await sqlContainer.StartAsync();

            factory = new CustomWebApplicationFactory(sqlContainer);

            Client = factory.CreateDefaultClient();

            var scoped = factory.Services.CreateScope();

            var config = scoped.ServiceProvider.GetService<Config>();

            CoreDataContext = scoped.ServiceProvider.GetService<CoreDataContext>();

            await SetupSSODatabase(scoped);
            var adminRoleId = await SetupCoreDatabase();

            await RegisterTestUser(Client, adminRoleId);
        }

        private async Task RegisterTestUser(HttpClient client, Guid adminRole)
        {
            var payload = new RegisterUser
            {
                AppId = 1,
                Culture = "",
                DisplayName = "Test",
                Email = "test@test.com",
                Password = "Test123!!",
                PhoneNumber = "07123 567831"
            };

            var response = await client.PostAsJsonAsync("api/Account/Register", payload);

            response.EnsureSuccessStatusCode();

            CoreDataContext.UserRoles.Add(new Objects.Entities.Security.UserRole { UserId = "test", RoleId = adminRole });

            await CoreDataContext.SaveChangesAsync();

            var loginResponse = await client.PostAsJsonAsync("api/Account/Login", new { User = "test@test.com", Pass = "Test123!!" });

            loginResponse.EnsureSuccessStatusCode();

            var token = await loginResponse.Content.ReadFromJsonAsync<Token>();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token.Id);
        }

        private async Task<Guid> SetupCoreDatabase()
        {
            var user = await CoreDataContext.AddAsync(new Objects.Entities.Security.User
            {
                Id = "Guest",
                Email = "guest@guest.com",
                DisplayName = "Guest",
                DefaultCultureId = ""
            });

            var app = await CoreDataContext.AddAsync(new App { DefaultTheme = "Default", DefaultCultureId = "", Domain = "localhost", Name = "Test App", ConfigJson = "{ }" });

            var userPrivileges = CoreDataContext
                .GetAllPrivileges()
                .Where(u => u.Id.Contains("read"))
                .Select(u => u.Id)
                .ToList();

            await CoreDataContext.AddAsync(new Objects.Entities.Security.Role { AppId = app.Id, Name = "Users", Privileges = userPrivileges });

            var adminPrivileges = CoreDataContext
                .GetAllPrivileges()
                .Select(u => u.Id)
                .ToList();

            var adminRole = await CoreDataContext.AddAsync(new Objects.Entities.Security.Role { AppId = app.Id, Name = "Administrators", Privileges = adminPrivileges });

            var role = await CoreDataContext.AddAsync(new Objects.Entities.Security.Role { AppId = app.Id, Name = "Guests", Privs = "folderrole_read,pagerole_read,userrole_read,appculture_read,page_read,folder_read,file_read,app_read" });

            await CoreDataContext.AddAsync(new Core.Objects.Entities.Security.UserRole { RoleId = role.Id, UserId = user.Id });

            await CoreDataContext.SaveChangesAsync();

            return adminRole.Id;
        }

        private static async Task SetupSSODatabase(IServiceScope scoped)
        {
            var ssoContext = scoped.ServiceProvider.GetService<ISecurityDbContextFactory>().CreateDbContext();

            await ssoContext.AddAsync(new Security.Objects.Entities.SSOUser
            {
                Id = "Guest",
                Email = "guest@guest.com",
                DisplayName = "Guest"
            });

            await ssoContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            if (sqlContainer != null)
            {
                await sqlContainer.StopAsync();
                await sqlContainer.DisposeAsync();
            }

            Client?.Dispose();

            factory?.Dispose();
        }
    }
}
