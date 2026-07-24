// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing;
using cCoder.Eventing.Models;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
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
                ApplyMailConfiguration(config, coreConfig);
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
                    CreateExternalReceiveProvider<ScheduledTask>(["scheduled_task_execute"]),
                    CreateQueuedFlowInstanceReceiveProvider(),
                ];
            });
        });
        builder.Services.RemoveAll<ISecurityDbContextFactory>();
        builder.Services.AddSingleton<ISecurityDbContextFactory>(
            new MSSQLSecurityDbContextFactory(config.GetValue<string>("ConnectionStrings:SSO"))
            {
                GetAuthInfo = _ => new SSOAuthInfo { SSOUserId = "Guest" },
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
                {
                    throw new InvalidOperationException(
                        "You must provide a workflow instance payload with a valid id.");
                }

                if (!string.Equals(message.Data?.State, "Queued", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                IWorkflowInstanceManagementOrchestrationService workflowInstanceManagementService =
                    serviceProvider.GetRequiredService<IWorkflowInstanceManagementOrchestrationService>();

                await workflowInstanceManagementService.ExecuteWaitingQueuedInstanceByIdAsync(
                    message.Data.Id);
            }
        };

    private static void ApplyMailConfiguration(
        IConfiguration config,
        cCoder.Core.Models.CoreConfiguration coreConfig)
    {
        coreConfig.MailGraphTenantId = ResolveSetting(
            config,
            "Mail:MicrosoftGraph:TenantId",
            "CCODER_MAIL_GRAPH_TENANT_ID");
        coreConfig.MailGraphClientId = ResolveSetting(
            config,
            "Mail:MicrosoftGraph:ClientId",
            "CCODER_MAIL_GRAPH_CLIENT_ID");
        coreConfig.MailGraphClientSecret = ResolveSetting(
            config,
            "Mail:MicrosoftGraph:ClientSecret",
            "CCODER_MAIL_GRAPH_CLIENT_SECRET");
        coreConfig.MailGraphBaseUrl = ResolveSetting(
            config,
            "Mail:MicrosoftGraph:GraphBaseUrl",
            "CCODER_MAIL_GRAPH_BASE_URL");
        coreConfig.MailGraphLoginBaseUrl = ResolveSetting(
            config,
            "Mail:MicrosoftGraph:LoginBaseUrl",
            "CCODER_MAIL_GRAPH_LOGIN_BASE_URL");
        coreConfig.MailGraphReceiveUser = ResolveSetting(
            config,
            "Mail:MicrosoftGraph:ReceiveUser",
            "CCODER_MAIL_INTEGRATION_RECEIVE_USER",
            "CCODER_MAIL_INTEGRATION_SEND_USER",
            "CCODER_MAIL_INTEGRATION_SMTP_USER");
        coreConfig.MailDefaultSenderProviderName = ResolveSetting(
            config,
            "Mail:DefaultSenderProviderName",
            "CCODER_MAIL_DEFAULT_SENDER_PROVIDER");
        coreConfig.MailDefaultReceiverProviderName = ResolveSetting(
            config,
            "Mail:DefaultReceiverProviderName",
            "CCODER_MAIL_DEFAULT_RECEIVER_PROVIDER");
    }

    private static string ResolveSetting(IConfiguration config, params string[] keys)
    {
        foreach (string key in keys)
        {
            string value = config.GetValue<string>(key);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}