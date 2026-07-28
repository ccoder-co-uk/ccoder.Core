// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Core.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Dependencies;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
using cCoder.Workflow.Services.Processings;

namespace HostedServices;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddHostedServices(
        this IServiceCollection services,
        IConfiguration applicationConfiguration,
        Action<CoreConfiguration> configure = null)
    {
        CoreConfiguration configuration = new();
        applicationConfiguration.Bind(configuration);
        configure?.Invoke(configuration);
        services.AddDependencies(configuration);
        services.AddOrchestrations();
        services.AddExposures();

        cCoder.Core.IServiceCollectionExtensions.AddCoreHostedServices(
            services,
            configuration);

        return services;
    }

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
        services.RemoveAll<IWorkflowInstanceProcessingService>();

        services.AddTransient<
            IWorkflowInstanceProcessingService,
            HostedServicesWorkflowInstanceManagementOrchestrationService>();
    }

    private static void AddExposures(
        this IServiceCollection services) =>
        services.AddHealthChecks();

}