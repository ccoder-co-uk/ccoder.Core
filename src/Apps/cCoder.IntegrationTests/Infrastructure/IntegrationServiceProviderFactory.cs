// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Dependencies;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
using Microsoft.Extensions.DependencyInjection;
using cCoder.IntegrationTests.Models;

namespace cCoder.IntegrationTests.Infrastructure;

internal static class IntegrationServiceProviderFactory
{
    public static ServiceProvider Create(AcceptanceSettings settings)
    {
        ServiceCollection services = new();
        services.AddLogging();

        services.AddSingleton(
implementationInstance:             new Config
            {
                ConnectionStrings = new Dictionary<string, string>
                {
                    ["Core"] = settings.CoreConnectionString,
                    ["SSO"] = settings.SsoConnectionString
                },
                Settings = new Dictionary<string, string>
                {
                    ["DecryptionKey"] = settings.DecryptionKey,
                    ["AggregateDomains"] = "false"
                },
                Services = new Dictionary<string, string>()
            });

        services.AddScoped<ISecurityDbContextFactory>(
implementationFactory:             provider => new MSSQLSecurityDbContextFactory(settings.SsoConnectionString)
            {
                GetAuthInfo = ignoreAuthInfo => ignoreAuthInfo
                    ? new SSOAuthInfo { SSOUserId = "Guest" }
                    : provider.GetService<ISSOAuthInfo>()
            });

        cCoder.Data.IServiceCollectionExtensions.AddCoreData(services: services,connectionString: settings.CoreConnectionString);

        return services.BuildServiceProvider(validateScopes: false);
    }
}