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


namespace Web.AcceptanceTests.Infrastructure;

internal sealed class WebAcceptanceFactory : WebApplicationFactory<Program>
{
    private readonly AcceptanceSettings settings;
    private readonly string originalHttpEventHubUrl;
    private readonly string originalHostedServicesUrl;
    private readonly string originalExternalEventingSetting;

    public WebAcceptanceFactory(AcceptanceSettings settings)
    {
        this.settings = settings;
        originalHttpEventHubUrl = Environment.GetEnvironmentVariable("Eventing__Http__HubUrl");
        originalHostedServicesUrl = Environment.GetEnvironmentVariable("Services__HostedServices");
        originalExternalEventingSetting = Environment.GetEnvironmentVariable("Settings__enableExternalEventing");

        Environment.SetEnvironmentVariable("Eventing__Http__HubUrl", null);
        Environment.SetEnvironmentVariable("Services__HostedServices", null);
        Environment.SetEnvironmentVariable("Settings__enableExternalEventing", "false");
    }

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
                new KeyValuePair<string, string>("Settings:enableExternalEventing", "false"),
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
                        ["enableExternalEventing"] = "false",
                    },
                    Services = new Dictionary<string, string>(),
                }
            );
            services.AddScoped<ISecurityDbContextFactory>(
                provider => new MSSQLSecurityDbContextFactory(settings.SsoConnectionString)
                {
                    GetAuthInfo = ignoreAuthInfo => ignoreAuthInfo
                        ? new SSOAuthInfo { SSOUserId = "Guest" }
                        : provider.GetService<ISSOAuthInfo>(),
                }
            );
            cCoder.Data.IServiceCollectionExtensions.AddCoreData(
                services,
                settings.CoreConnectionString
            );
        });
    }

    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable("Eventing__Http__HubUrl", originalHttpEventHubUrl);
        Environment.SetEnvironmentVariable("Services__HostedServices", originalHostedServicesUrl);
        Environment.SetEnvironmentVariable("Settings__enableExternalEventing", originalExternalEventingSetting);
        base.Dispose(disposing);
    }
}



