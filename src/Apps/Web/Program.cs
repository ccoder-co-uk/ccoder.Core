using cCoder.Core;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Http;
using cCoder.Eventing.Models;

namespace Web;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IConfiguration config = ConfigureApplication(builder.Configuration, builder.Environment);

        builder.Services.AddCoreWeb(coreBuilder =>
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
                coreConfig.EnableHttpEventing = IsHttpEventProvider(coreConfig.EventProviderType)
                    && !string.IsNullOrWhiteSpace(coreConfig.HttpEventHubUrl);
                coreConfig.EnableServiceBusEventing = IsServiceBusEventProvider(coreConfig.EventProviderType)
                    && !string.IsNullOrWhiteSpace(coreConfig.ServiceBusConnectionString);
                coreConfig.MaxConcurrency = ResolveMaxConcurrency(config, coreConfig.EventProviderType);
                coreConfig.DebugInfo = config.GetValue<bool>("DebugInfo");
                coreConfig.LogSQL = config.GetValue<bool>("LogSQL");

                if (coreConfig.EnableHttpEventing || coreConfig.EnableServiceBusEventing)
                {
                    coreConfig.EventProviders =
                    [
                        CreateExternalSendProvider<App>(
                            coreConfig.EventProviderType,
                            IsServiceBusEventProvider(coreConfig.EventProviderType)
                                ? ["app_add", "app_update"]
                                : ["app_add", "app_update", "app_delete"]),
                        CreateExternalSendProvider<Folder>(
                            coreConfig.EventProviderType,
                            IsServiceBusEventProvider(coreConfig.EventProviderType)
                                ? []
                                : ["folder_delete"]),
                        CreateExternalSendProvider<FlowInstanceData>(coreConfig.EventProviderType, ["flow_instance_data_add"])
                    ];
                }
            });
        });

        WebApplication app = builder.Build();
        app.StartCoreWeb();
        app.Run();
    }

    private static IConfiguration ConfigureApplication(ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.testing.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return configuration;
    }

    private static EventProvider<T> CreateExternalSendProvider<T>(
        string eventProviderType,
        string[] eventNames) =>
        new()
        {
            Events = eventNames,
            SendHandler = async (serviceProvider, eventName, message) =>
            {
                if (IsServiceBusEventProvider(eventProviderType))
                {
                    IAzureServiceBusEventHub serviceBusEventHub =
                        serviceProvider.GetRequiredService<IAzureServiceBusEventHub>();

                    await serviceBusEventHub.RaiseEventAsync(
                        eventName,
                        new ServiceBusEventMessage<T>
                        {
                            AuthInfo = new ServiceBusEventAuthInfo
                            {
                                SSOUserId = message.AuthInfo?.SSOUserId ?? string.Empty
                            },
                            Data = message.Data
                        });

                    return;
                }

                IHttpEventHub httpEventHub = serviceProvider.GetRequiredService<IHttpEventHub>();
                await httpEventHub.RaiseEventAsync(eventName, message);
            }
        };

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
}
