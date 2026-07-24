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
using cCoder.Security.Data.EF.Dependencies;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
using cCoder.Workflow.Services.Processings;

namespace HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args: args);
        IConfiguration config = ConfigureApplication(configuration: builder.Configuration,environment: builder.Environment);

        builder.Services.AddCoreHostedServices(configure: coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(configure: coreConfig =>
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
implementationInstance:             new MSSQLSecurityDbContextFactory(config.GetValue<string>(key: "ConnectionStrings:SSO"))
            {
                GetAuthInfo = _ => new SSOAuthInfo { SSOUserId = "Guest" },
            });

        builder.Services.RemoveAll<IWorkflowInstanceProcessingService>();

        builder.Services.AddTransient<
            IWorkflowInstanceProcessingService,
            HostedServicesWorkflowInstanceManagementOrchestrationService>();

        WebApplication app = builder.Build();
        app.StartCoreHostedServices();
        app.Run();
    }

    private static IConfiguration ConfigureApplication(ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "appsettings.json",optional: false,reloadOnChange: true)
            .AddJsonFile(path: $"appsettings.{environment.EnvironmentName}.json",optional: true,reloadOnChange: true)
            .AddEnvironmentVariables();

        return configuration;
    }

    private static string ResolveEventProviderType(IConfiguration config) =>
        config.GetValue<string>(key: "Eventing:ProviderType")
        ?? config.GetValue<string>(key: "Eventing:Provider")
        ?? "Http";

    private static int ResolveMaxConcurrency(IConfiguration config, string eventProviderType) =>
        IsServiceBusEventProvider(eventProviderType: eventProviderType)
            ? config.GetValue<int?>(key: "Eventing:ServiceBus:MaxConcurrency") ?? 1
            : config.GetValue<int?>(key: "Eventing:Http:MaxConcurrency") ?? 1;

    private static bool IsServiceBusEventProvider(string eventProviderType) =>
        string.Equals(a: eventProviderType,b: "ServiceBus",comparisonType: StringComparison.OrdinalIgnoreCase);

    private static bool IsHttpEventProvider(string eventProviderType) =>
        string.Equals(a: eventProviderType,b: "Http",comparisonType: StringComparison.OrdinalIgnoreCase);

    private static EventProvider<T> CreateExternalReceiveProvider<T>(string[] eventNames) =>
        new()
        {
            Events = eventNames,
            ReceiveHandler = async (serviceProvider, eventName, message) =>
            {
                IEventHub eventHub = serviceProvider.GetRequiredService<IEventHub>();

                await eventHub.RaiseEventAsync(
name:                     eventName,message:                     new EventMessage<T>
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

                if (!string.Equals(a: message.Data?.State,b: "Queued",comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                IWorkflowInstanceProcessingService workflowInstanceProcessingService =
                    serviceProvider.GetRequiredService<IWorkflowInstanceProcessingService>();

                await workflowInstanceProcessingService.ExecuteWaitingQueuedInstanceByIdAsync(
                    flowInstanceDataId: message.Data.Id);
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
            string value = config.GetValue<string>(key: key);

            if (!string.IsNullOrWhiteSpace(value: value))
            {
                return value;
            }
        }

        return null;
    }
}