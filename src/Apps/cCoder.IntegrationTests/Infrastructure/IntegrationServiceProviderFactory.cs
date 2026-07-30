// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Dependencies;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Models.Configurations;
using Microsoft.Extensions.DependencyInjection;
using cCoder.IntegrationTests.Models;

namespace cCoder.IntegrationTests.Infrastructure;

internal static class IntegrationServiceProviderFactory
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
                    : provider.GetService<ISSOAuthInfo>()
            });

        services.AddData(configuration: new DataConfiguration
        {
            ConnectionString = settings.CoreConnectionString
        });

        return services.BuildServiceProvider(validateScopes: false);
    }
}