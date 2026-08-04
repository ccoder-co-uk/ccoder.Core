// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Core.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Dependencies;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Models.Configurations;
using cCoder.Workflow.Exposures;

namespace HostedServices;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddHostedServices(
        this IServiceCollection services,
        IConfiguration applicationConfiguration,
        Action<CoreConfiguration> configure = null)
    {
        CoreConfiguration configuration =
            CoreConfigurationFactory.Create(applicationConfiguration);
        configure?.Invoke(configuration);
        services.AddApplicationLogging(applicationConfiguration);
        services.AddDependencies(configuration);
        services.AddOrchestrations();
        services.AddExposures();

        cCoder.Core.IServiceCollectionExtensions.AddCoreHostedServices(
            services,
            configuration);

        return services;
    }

    private static void AddApplicationLogging(
        this IServiceCollection services,
        IConfiguration applicationConfiguration) =>
        services.AddLogging(configure: logBuilder =>
        {
            logBuilder.ClearProviders();
            logBuilder.AddFilter(
                levelFilter: level => level >= LogLevel.Debug);

            logBuilder.AddSimpleConsole(configure: options =>
            {
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
                options.SingleLine = true;
            });

            logBuilder.AddConfiguration(
                configuration:
                    applicationConfiguration.GetSection(key: "Logging"));
        });

    private static void AddDependencies(
        this IServiceCollection services,
        CoreConfiguration configuration)
    {
        services.RemoveAll<ISecurityDbContextFactory>();

        services.AddSingleton<ISecurityDbContextFactory>(
            implementationInstance: new MSSQLSecurityDbContextFactory(
                configuration.Security.ConnectionString)
            {
                GetAuthInfo = _ => new SSOAuthInfo { SSOUserId = "Guest" },
            });
    }

    private static void AddOrchestrations(
        this IServiceCollection services)
    {
        services.RemoveAll<IWorkflowInstanceManager>();

        services.AddTransient<
            IWorkflowInstanceManager,
            HostedServicesWorkflowInstanceManagementOrchestrationService>();
    }

    private static void AddExposures(
        this IServiceCollection services) =>
        services.AddHealthChecks();

}