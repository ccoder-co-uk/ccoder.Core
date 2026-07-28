// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Dependencies;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly string originalAggregateDomainsSetting;

    public WebAcceptanceFactory(AcceptanceSettings settings)
    {
        this.settings = settings;
        originalHttpEventHubUrl = Environment.GetEnvironmentVariable(
            variable: "Eventing__Http__HubUrl");
        originalHostedServicesUrl = Environment.GetEnvironmentVariable(variable: "Services__HostedServices");
        originalExternalEventingSetting = Environment.GetEnvironmentVariable(variable: "Settings__enableExternalEventing");
        originalAggregateDomainsSetting = Environment.GetEnvironmentVariable(
            variable: "AppSecurity__AggregateDomains");

        Environment.SetEnvironmentVariable(
            variable: "Eventing__Http__HubUrl",
            value: null);
        Environment.SetEnvironmentVariable(variable: "Services__HostedServices",value: null);
        Environment.SetEnvironmentVariable(variable: "Settings__enableExternalEventing",value: "false");
        Environment.SetEnvironmentVariable(
            variable: "AppSecurity__AggregateDomains",
            value: settings.AggregateDomains.ToString());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment: "Acceptance");

        builder.ConfigureAppConfiguration(configureDelegate: (_, config) =>
        {
            config.AddInMemoryCollection(
initialData:             [
                new KeyValuePair<string, string>("AppSecurity:ConnectionString", settings.CoreConnectionString),
                new KeyValuePair<string, string>("Security:ConnectionString", settings.SsoConnectionString),
                new KeyValuePair<string, string>("Security:DecryptionKey", settings.DecryptionKey),
                new KeyValuePair<string, string>("AppSecurity:AggregateDomains", settings.AggregateDomains.ToString()),
                new KeyValuePair<string, string>("ContentManagement:ConnectionString", settings.CoreConnectionString),
                new KeyValuePair<string, string>("DocumentManagement:ConnectionString", settings.CoreConnectionString),
                new KeyValuePair<string, string>("Logging:ConnectionString", settings.CoreConnectionString),
                new KeyValuePair<string, string>("Mail:ConnectionString", settings.CoreConnectionString),
                new KeyValuePair<string, string>("Workflow:ConnectionString", settings.CoreConnectionString),
                new KeyValuePair<string, string>("Settings:enableExternalEventing", "false"),
                new KeyValuePair<string, string>("Eventing:Http:HubUrl", string.Empty),
                new KeyValuePair<string, string>("DebugInfo", "true"),
            ]);
        });

        builder.ConfigureServices(configureServices: services =>
        {
            services.AddOptions();

            services.Replace(
                descriptor: ServiceDescriptor.Singleton<
                    IDistributedCache,
                    MemoryDistributedCache>());

            services.RemoveAll<Config>();
            services.RemoveAll<ICoreContextFactory>();
            services.RemoveAll<ISecurityDbContextFactory>();

            services.AddSingleton(
implementationInstance:                 new Config
                {
                    ConnectionStrings = new Dictionary<string, string>
                    {
                        ["Core"] = settings.CoreConnectionString,
                        ["SSO"] = settings.SsoConnectionString,
                    },
                    Settings = new Dictionary<string, string>
                    {
                        ["DecryptionKey"] = settings.DecryptionKey,
                        ["AggregateDomains"] = settings.AggregateDomains.ToString(),
                        ["enableExternalEventing"] = "false",
                    },
                    Services = new Dictionary<string, string>(),
                    DebugInfo = true,
                }
            );

            services.AddScoped<ISecurityDbContextFactory>(
implementationFactory:                 provider => new MSSQLSecurityDbContextFactory(settings.SsoConnectionString)
                {
                    GetAuthInfo = ignoreAuthInfo => ignoreAuthInfo
                        ? new SSOAuthInfo { SSOUserId = "Guest" }
                        : provider.GetService<ISSOAuthInfo>(),
                }
            );

            cCoder.Data.IServiceCollectionExtensions.AddCoreData(
services:                 services,connectionString:                 settings.CoreConnectionString
            );
        });
    }

    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable(
            variable: "Eventing__Http__HubUrl",
            value: originalHttpEventHubUrl);
        Environment.SetEnvironmentVariable(variable: "Services__HostedServices",value: originalHostedServicesUrl);
        Environment.SetEnvironmentVariable(variable: "Settings__enableExternalEventing",value: originalExternalEventingSetting);
        Environment.SetEnvironmentVariable(
            variable: "AppSecurity__AggregateDomains",
            value: originalAggregateDomainsSetting);
        base.Dispose(disposing: disposing);
    }
}