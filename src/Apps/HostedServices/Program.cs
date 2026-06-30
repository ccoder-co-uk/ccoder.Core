using cCoder.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing;
using cCoder.Eventing.Models;
using cCoder.Workflow.Services.Orchestrations;

namespace HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IConfiguration config = ConfigureApplication(builder.Configuration, builder.Environment);

        builder.Services.AddCoreHostedServices(coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(coreConfig =>
            {
                coreConfig.CoreConnectionString = config.GetValue<string>("ConnectionStrings:Core");
                coreConfig.SecurityConnectionString = config.GetValue<string>("ConnectionStrings:SSO");
                coreConfig.DecryptionKey = config.GetValue<string>("Settings:DecryptionKey");
                coreConfig.CacheSource = config.GetValue<string>("Settings:CacheSource");
                coreConfig.CacheSourceAppId = config.GetValue<int?>("Settings:CacheSourceAppId");
                coreConfig.CacheExpiry = config.GetValue<int?>("Settings:CacheExpiry");
                coreConfig.SslPort = config.GetValue<int?>("Settings:sslPort");
                coreConfig.WorkflowServiceUrl = config.GetValue<string>("Services:Workflow");
                coreConfig.EventProviderType = ResolveEventProviderType(config);
                coreConfig.HttpEventHubUrl = HttpEventHubUrlResolver.Resolve(config);
                coreConfig.ServiceBusConnectionString = config.GetConnectionString("ServiceBus");
                coreConfig.EnableHttpEventing = IsHttpEventProvider(coreConfig.EventProviderType);
                coreConfig.EnableServiceBusEventing = IsServiceBusEventProvider(coreConfig.EventProviderType)
                    && !string.IsNullOrWhiteSpace(coreConfig.ServiceBusConnectionString);
                coreConfig.MaxConcurrency = ResolveMaxConcurrency(config, coreConfig.EventProviderType);
                coreConfig.DebugInfo = config.GetValue<bool>("DebugInfo");
                coreConfig.LogSQL = config.GetValue<bool>("LogSQL");
                coreConfig.EventProviders =
                [
                    CreateExternalReceiveProvider<App>(["app_add", "app_update", "app_delete"]),
                    CreateExternalReceiveProvider<Folder>(["folder_delete"]),
                    CreateQueuedFlowInstanceReceiveProvider(),
                ];
            });
        });
        builder.Services.RemoveAll<IWorkflowInstanceManagementOrchestrationService>();
        builder.Services.AddTransient<IWorkflowInstanceManagementOrchestrationService, HostedServicesWorkflowInstanceManagementOrchestrationService>();

        WebApplication app = builder.Build();
        app.StartCoreHostedServices();
        app.Run();
    }

    private static IConfiguration ConfigureApplication(ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return configuration;
    }

    private static string ResolveEventProviderType(IConfiguration config) =>
        config.GetValue<string>("Eventing:ProviderType")
        ?? config.GetValue<string>("Eventing:Provider")
        ?? "Http";

    private static int ResolveMaxConcurrency(IConfiguration config, string eventProviderType) =>
        IsServiceBusEventProvider(eventProviderType)
            ? config.GetValue<int?>("Eventing:ServiceBus:MaxConcurrency") ?? 1
            : config.GetValue<int?>("Eventing:Http:MaxConcurrency") ?? 1;

    private static bool IsServiceBusEventProvider(string eventProviderType) =>
        string.Equals(eventProviderType, "ServiceBus", StringComparison.OrdinalIgnoreCase);

    private static bool IsHttpEventProvider(string eventProviderType) =>
        string.Equals(eventProviderType, "Http", StringComparison.OrdinalIgnoreCase);

    private static EventProvider<T> CreateExternalReceiveProvider<T>(string[] eventNames) =>
        new()
        {
            Events = eventNames,
            ReceiveHandler = async (serviceProvider, eventName, message) =>
            {
                IEventHub eventHub = serviceProvider.GetRequiredService<IEventHub>();

                await eventHub.RaiseEventAsync(
                    eventName,
                    new EventMessage<T>
                    {
                        AuthInfo = new EventAuthInfo
                        {
                            SSOUserId = message.AuthInfo?.SSOUserId ?? "Guest",
                        },
                        Data = message.Data,
                    });
            }
        };

    private static EventProvider<FlowInstanceData> CreateQueuedFlowInstanceReceiveProvider() =>
        new()
        {
            Events = ["flow_instance_data_add"],
            ReceiveHandler = async (serviceProvider, _, message) =>
            {
                if (message.Data?.Id == Guid.Empty)
                    throw new InvalidOperationException(
                        "You must provide a workflow instance payload with a valid id.");

                if (!string.Equals(message.Data?.State, "Queued", StringComparison.OrdinalIgnoreCase))
                    return;

                IWorkflowInstanceManagementOrchestrationService workflowInstanceManagementService =
                    serviceProvider.GetRequiredService<IWorkflowInstanceManagementOrchestrationService>();

                await workflowInstanceManagementService.ExecuteWaitingQueuedInstanceByIdAsync(
                    message.Data.Id);
            }
        };
}
