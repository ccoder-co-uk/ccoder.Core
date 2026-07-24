// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Dependencies;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
using HostedServices.AcceptanceTests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal static class AcceptanceServiceProviderFactory
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
                    ["DecryptionKey"] = settings.DecryptionKey
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