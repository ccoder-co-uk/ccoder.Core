// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Workflow.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workflow.Models;
using Workflow.Exposures;
using Workflow.Brokers.Loggings;
using Workflow.Services.Processings.WorkflowFunctions;

namespace Workflow;

internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflow(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AppConfiguration> configure = null)
    {
        AppConfiguration appConfiguration = new();
        configuration.Bind(instance: appConfiguration);
        configure?.Invoke(obj: appConfiguration);

        services.AddProcessings();
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddData(configuration: appConfiguration.CoreData);
        services.AddWorkflowEngineHostedServices();

        return services;
    }

    private static void AddProcessings(
        this IServiceCollection services)
    {
        services.AddTransient<
            IWorkflowFunctionsProcessingService,
            WorkflowFunctionsProcessingService>();

        services.AddTransient<
            IWorkflowFunctionsManager,
            WorkflowFunctionsProcessingService>();
    }
}