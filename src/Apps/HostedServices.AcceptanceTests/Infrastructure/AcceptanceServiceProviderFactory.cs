// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Security.Data.EF;
using cCoder.Security.Models.Configurations;
using HostedServices.AcceptanceTests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal static class AcceptanceServiceProviderFactory
{
    public static ServiceProvider Create(AcceptanceSettings settings)
    {
        ServiceCollection services = new();
        services.AddLogging();

        services.AddSecurityData(configure: configuration =>
            configuration.ConnectionString = settings.SsoConnectionString);

        services.AddData(
            configuration: new DataConfiguration
            {
                ConnectionString = settings.CoreConnectionString,
            });

        return services.BuildServiceProvider(validateScopes: false);
    }
}