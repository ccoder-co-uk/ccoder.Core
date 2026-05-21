using cCoder.Data;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Web.AcceptanceTests.Models;
using HostedServicesProgram = HostedServices.Program;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal sealed class HostedServicesAcceptanceFactory
    : WebApplicationFactory<HostedServicesProgram>
{
    private readonly AcceptanceSettings settings;

    public HostedServicesAcceptanceFactory(AcceptanceSettings settings) =>
        this.settings = settings;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Acceptance");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
            [
                new KeyValuePair<string, string>("ConnectionStrings:Core", settings.CoreConnectionString),
                new KeyValuePair<string, string>("ConnectionStrings:SSO", settings.SsoConnectionString),
                new KeyValuePair<string, string>("Settings:DecryptionKey", settings.DecryptionKey),
                new KeyValuePair<string, string>("Eventing:Http:HubUrl", string.Empty),
            ]);
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<Config>();
            services.RemoveAll<ICoreContextFactory>();
            services.RemoveAll<ISecurityDbContextFactory>();
            services.AddSingleton(
                new Config
                {
                    ConnectionStrings = new Dictionary<string, string>
                    {
                        ["Core"] = settings.CoreConnectionString,
                        ["SSO"] = settings.SsoConnectionString,
                    },
                    Settings = new Dictionary<string, string>
                    {
                        ["DecryptionKey"] = settings.DecryptionKey,
                    },
                    Services = new Dictionary<string, string>(),
                });
            services.AddScoped<ISecurityDbContextFactory>(
                provider => new MSSQLSecurityDbContextFactory(settings.SsoConnectionString)
                {
                    GetAuthInfo = ignoreAuthInfo => ignoreAuthInfo
                        ? new SSOAuthInfo { SSOUserId = "Guest" }
                        : provider.GetService<ISSOAuthInfo>(),
                });
            cCoder.Data.IServiceCollectionExtensions.AddCoreData(
                services,
                settings.CoreConnectionString);
        });
    }
}
