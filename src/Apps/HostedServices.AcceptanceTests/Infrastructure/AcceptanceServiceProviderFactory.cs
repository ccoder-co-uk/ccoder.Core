// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models;
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

        services.AddScoped<ISecurityDbContextFactory>(
            implementationFactory: provider =>
                new MSSQLSecurityDbContextFactory(
                    settings.SsoConnectionString)
                {
                    GetAuthInfo = ignoreAuthInfo => ignoreAuthInfo
                        ? new SSOAuthInfo { SSOUserId = "Guest" }
                        : provider.GetService<ISSOAuthInfo>(),
                });

        services.AddData(
            configuration: new DataConfiguration
            {
                ConnectionString = settings.CoreConnectionString,
            });

        return services.BuildServiceProvider(validateScopes: false);
    }
}