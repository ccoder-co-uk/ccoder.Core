// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Workflow.Services.Processings.WorkflowFunctions;

namespace Workflow;

internal static class IServiceCollectionExtensions
{
    internal static IServiceCollection AddWorkflowApplication(
        this IServiceCollection services)
    {
        services.AddWorkflowEngine();

        services.AddTransient<
            IWorkflowFunctionsProcessingService,
            WorkflowFunctionsProcessingService>();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .SetBasePath(basePath: Directory.GetCurrentDirectory())
            .AddJsonFile(path: "host.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddLogging(configure: loggingBuilder =>
        {
            loggingBuilder.ClearProviders();

            loggingBuilder.AddSimpleConsole(
                configure: options => options.SingleLine = true);

            loggingBuilder.AddFilter(
                levelFilter: level => level >= LogLevel.Debug);

            loggingBuilder.AddConfiguration(
                configuration: configuration.GetSection(key: "logging"));
        });

        return services;
    }
}