using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

namespace cCoder.Core.E2E.IntegrationTests
{
    internal class CustomWebApplicationFactory(MsSqlContainer sqlContainer) : WebApplicationFactory<Web.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(sqlContainer.GetConnectionString());

            string port = connectionStringBuilder.DataSource.Split(",").Last();

            connectionStringBuilder.InitialCatalog = "dev-Members";

            string membersConnectionString = connectionStringBuilder.ConnectionString;

            connectionStringBuilder.InitialCatalog = "dev-Core";

            string coreConnectionString = connectionStringBuilder.ConnectionString;

            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new KeyValuePair<string, string?>("ConnectionStrings:SSO", membersConnectionString),
                    new KeyValuePair<string, string?>("ConnectionStrings:Core", coreConnectionString)
                ])
                .Build();

            builder.UseConfiguration(configBuilder);

            builder.ConfigureAppConfiguration((context, config) =>
            {
                var k = context.Configuration;

                k.GetSection("ConnectionStrings")["Core"] = coreConnectionString;
                k.GetSection("ConnectionStrings")["SSO"] = membersConnectionString;

                // Add the in-memory configuration to override the default config
                config.AddInMemoryCollection(
                [
                    new KeyValuePair<string, string?>("ConnectionStrings:SSO", membersConnectionString),
                    new KeyValuePair<string, string?>("ConnectionStrings:Core", coreConnectionString)
                ]);
            });

            base.ConfigureWebHost(builder);
        }
    }
}
