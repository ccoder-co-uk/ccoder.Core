// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Workflow.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workflow.Models;
using Workflow.Exposures;
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

    private static void AddDependencies(this IServiceCollection services) =>
        services.AddTransient<WorkflowFunctionsDependency>();

    private static void AddBrokers(this IServiceCollection services)
    {
        services.AddTransient<IWorkflowFunctionsBroker, WorkflowFunctionsBroker>();
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