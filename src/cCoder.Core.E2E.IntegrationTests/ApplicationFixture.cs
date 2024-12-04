using cCoder.Core.Objects;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using cCoder.Core.Data;
using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.E2E.IntegrationTests
{
    public class ApplicationFixture : IAsyncLifetime
    {
#pragma warning disable CS8618
        WebApplicationFactory<Web.Program> factory;
        MsSqlContainer sqlContainer;
        public HttpClient Client;
#pragma warning restore CS8618 // Seems to not understand IAsyncLifetime

        public async Task InitializeAsync()
        {
            sqlContainer = new MsSqlBuilder()
                .WithPortBinding(1433, assignRandomHostPort: true)
                .Build();

            await sqlContainer.StartAsync();

            factory = new CustomWebApplicationFactory(sqlContainer);

            Client = factory.CreateDefaultClient();

            var scoped = factory.Services.CreateScope();

            var config = scoped.ServiceProvider.GetService<Config>();

            var coreDataContext = scoped.ServiceProvider.GetService<CoreDataContext>();
            
            coreDataContext.Database.EnsureCreated();

            var ssoContext = scoped.ServiceProvider.GetService<ISecurityDbContextFactory>().CreateDbContext();

            ssoContext.Database.EnsureCreated();

            var app = await coreDataContext.AddAsync(new App { DefaultTheme = "Default", DefaultCultureId = "", Domain = "localhost", Name = "Test App", ConfigJson = "{ }" });

            var user = await coreDataContext.AddAsync(new Objects.Entities.Security.User
            {
                Id = "Guest",
                Email = "guest@guest.com",
                DisplayName = "Guest",
                DefaultCultureId = ""
            });
            
            var role = await coreDataContext.AddAsync(new Objects.Entities.Security.Role { AppId = app.Id, Name = "Guest Privs", Privileges = ["app_read"] });
            
            await coreDataContext.AddAsync(new Core.Objects.Entities.Security.UserRole { RoleId = role.Id, UserId = user.Id });
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
