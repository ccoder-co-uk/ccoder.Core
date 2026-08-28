// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Core.Models;
using Web.Models;
using Web.Services.Processings;
using Web.Exposures;
using Web.Services.Aggregations;
using Web.Brokers.Api;
using Web.Services.Foundations.Api;
using Web.Services.Orchestrations.Api;
using Web.Dependencies.Api;

namespace Web;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddWeb(
        this IServiceCollection services,
        IConfiguration applicationConfiguration,
        Action<AppConfiguration> configure = null)
    {
        AppConfiguration configuration =
            CoreConfigurationFactory.Create<AppConfiguration>(
                configuration: applicationConfiguration);
        configure?.Invoke(configuration);
        services.AddApplicationLogging(applicationConfiguration);
        services.AddBrokers();
        services.AddFoundations();
        services.AddProcessings();
        services.AddAggregations();
        services.AddOrchestrations();
        services.AddExposures();

        cCoder.Core.IServiceCollectionExtensions.AddCoreWeb(
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

    private static void AddBrokers(this IServiceCollection services)
    {
        services.AddScoped<ApiScriptExecutionDependency>();
        services.AddScoped<ICommonObjectCacheBroker, CommonObjectCacheBroker>();
        services.AddScoped<IMetadataCacheBroker, MetadataCacheBroker>();
        services.AddScoped<IApiScriptExecutionBroker, ApiScriptExecutionBroker>();
        services.AddScoped<IApiContextBroker, ApiContextBroker>();
    }

    private static void AddFoundations(this IServiceCollection services)
    {
        services.AddScoped<
            IApiScriptAuthorizationService,
            ApiScriptAuthorizationService>();

        services.AddScoped<
            IApiScriptExecutionService,
            ApiScriptExecutionService>();

        services.AddScoped<IApiContextService, ApiContextService>();
    }

    private static void AddProcessings(this IServiceCollection services) =>
        services.AddScoped<
            IHomeSessionProcessingService,
            HomeSessionProcessingService>();

    private static void AddAggregations(this IServiceCollection services) =>
        services.AddScoped<
            IApiCacheAggregationService,
            ApiCacheAggregationService>();

    private static void AddOrchestrations(this IServiceCollection services) =>
        services.AddScoped<
            IApiScriptOrchestrationService,
            ApiScriptOrchestrationService>();

    private static void AddExposures(this IServiceCollection services)
    {
        services.AddHealthChecks();
        services.AddScoped<IHomeSessionManager, HomeSessionManager>();
        services.AddScoped<IApiCacheManager, ApiCacheManager>();
        services.AddScoped<IApiScriptManager, ApiScriptManager>();
        services.AddScoped<IApiContextManager, ApiContextManager>();
    }

}