// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Workflow.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workflow.Models;
using Workflow.Services.Processings.WorkflowFunctions;

namespace Workflow;

internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflow(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<WorkflowConfiguration> configure = null)
    {
        WorkflowConfiguration workflowConfiguration = new();
        configuration.Bind(instance: workflowConfiguration);
        configure?.Invoke(obj: workflowConfiguration);

        services.AddProcessings();
        services.AddData(configuration: workflowConfiguration.Data);
        services.AddWorkflowEngineHostedServices();

        return services;
    }

    private static void AddProcessings(
        this IServiceCollection services) =>
        services.AddTransient<
            IWorkflowFunctionsProcessingService,
            WorkflowFunctionsProcessingService>();
}