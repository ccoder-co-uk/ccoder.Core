using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Workflow;
using Workflow.Services;

IHost host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureLogging(loggingBuilder =>
    {
        IConfigurationRoot configRoot = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("host.json", optional: false, reloadOnChange: true)
            .Build();

        loggingBuilder.ClearProviders();
        loggingBuilder.AddSimpleConsole(options => options.SingleLine = true);
        loggingBuilder.AddFilter(level => level >= LogLevel.Debug);
        loggingBuilder.AddConfiguration(configRoot.GetSection("logging"));
    })
    .ConfigureServices(services =>
    {
        services.AddTransient<FlowRunner>();
        services.AddTransient<WorkflowExecutionService>();
        services.AddTransient<WorkflowScriptExecutionService>();
    })
    .Build();

await host.RunAsync();
