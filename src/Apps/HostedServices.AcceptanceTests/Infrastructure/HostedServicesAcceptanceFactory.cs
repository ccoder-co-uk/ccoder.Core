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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HostedServices.AcceptanceTests.Models;
using HostedServicesProgram = HostedServices.Program;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal sealed class HostedServicesAcceptanceFactory(AcceptanceSettings settings)
        : WebApplicationFactory<HostedServicesProgram>
{
    private readonly AcceptanceSettings settings = settings;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment: "Acceptance");

        builder.ConfigureAppConfiguration(configureDelegate: (_, config) =>
        {
            config.AddInMemoryCollection(
initialData: [
                new KeyValuePair<string, string>("ConnectionStrings:Core", settings.CoreConnectionString),
                new KeyValuePair<string, string>("ConnectionStrings:SSO", settings.SsoConnectionString),
                new KeyValuePair<string, string>("Settings:DecryptionKey", settings.DecryptionKey),
                new KeyValuePair<string, string>("Eventing:Http:HubUrl", string.Empty),
            ]);
        });

        builder.ConfigureServices(configureServices: services =>
        {
            services.RemoveAll<Config>();
            services.RemoveAll<ICoreContextFactory>();
            services.RemoveAll<ISecurityDbContextFactory>();

            services.AddSingleton(
implementationInstance: new Config
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
implementationFactory: _ => new MSSQLSecurityDbContextFactory(settings.SsoConnectionString)
{
    GetAuthInfo = _ => new SSOAuthInfo { SSOUserId = "Guest" },
});

            cCoder.Data.IServiceCollectionExtensions.AddCoreData(
services: services, connectionString: settings.CoreConnectionString);
        });
    }
}