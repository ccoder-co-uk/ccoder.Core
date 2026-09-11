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
using Workflow.Brokers.WorkflowFunctions;
using Workflow.Dependencies;
using Workflow.Services.Foundations.WorkflowFunctions;
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

        services.AddDependencies();
        services.AddBrokers();
        services.AddFoundations();
        services.AddProcessings();
        services.AddData(configuration: appConfiguration.CoreData);
        services.AddWorkflowEngineHostedServices();

        return services;
    }

    private static void AddDependencies(this IServiceCollection services)
    {
        services.AddTransient<WorkflowFunctionHttpDependency>();
        services.AddTransient<WorkflowJsonDependency>();
    }

    private static void AddBrokers(this IServiceCollection services)
    {
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddTransient<IWorkflowFunctionHttpBroker, WorkflowFunctionHttpBroker>();
        services.AddTransient<IWorkflowJsonBroker, WorkflowJsonBroker>();
        services.AddTransient<IWorkflowRunnerBroker, WorkflowRunnerBroker>();

        services.AddTransient<
            IWorkflowScriptExecutionBroker,
            WorkflowScriptExecutionBroker>();
    }

    private static void AddFoundations(this IServiceCollection services) =>
        services.AddTransient<IWorkflowFunctionsService, WorkflowFunctionsService>();

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