// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;
using cCoder.Data;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Http;
using cCoder.Eventing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Web.Dependencies.Filters;
using Web.Services.Processings;
using Web.Exposures;

namespace Web;

internal static class IServiceCollectionExtensions
{
    internal static IServiceCollection AddWeb(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        IConfiguration config = ConfigureApplication(configuration: builder.Configuration,environment: builder.Environment);

        builder.Services.AddCoreWeb(configure: coreBuilder =>
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
                coreConfig.AggregateDomains = config.GetValue<bool>("Settings:AggregateDomains");
                coreConfig.WorkflowServiceUrl = config.GetValue<string>("Services:Workflow");
                ApplyMailConfiguration(config, coreConfig);
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
                    List<EventProvider> providers =
                    [
                        CreateExternalSendProvider<App>(
                            coreConfig.EventProviderType,
                            IsServiceBusEventProvider(coreConfig.EventProviderType)
                                ? ["app_add", "app_update"]
                                : ["app_add", "app_update"]),
                        CreateExternalSendProvider<Folder>(
                            coreConfig.EventProviderType,
                            IsServiceBusEventProvider(coreConfig.EventProviderType)
                                ? []
                                : ["folder_delete"]),
                        CreateExternalSendProvider<ScheduledTask>(
                            coreConfig.EventProviderType,
                            ["scheduled_task_execute"]),
                        CreateExternalSendProvider<FlowInstanceData>(coreConfig.EventProviderType, ["flow_instance_data_add"])
                    ];

                    if (IsHttpEventProvider(coreConfig.EventProviderType))
                    {
                        providers.Add(CreateAppDeleteExternalSendProvider());
                    }

                    coreConfig.EventProviders = [.. providers];
                }
            });
        });

        services.AddHealthChecks();
        services.AddScoped<HomeDefaultsActionFilter>();
        services.AddScoped<HomeExceptionFilter>();
        services.AddScoped<
            IHomeSessionProcessingService,
            HomeSessionProcessingService>();
        services.AddScoped<IHomeSessionManager, HomeSessionManager>();

        return services;
    }

    internal static WebApplication MapWebHealth(
        this WebApplication app)
    {
        app.MapHealthChecks(
            pattern: "/Health",
            options: new HealthCheckOptions
            {
                ResponseWriter = async (context, _) =>
                    await context.Response.WriteAsync(text: "OK")
            });

        return app;
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

    private static EventProvider<T> CreateExternalSendProvider<T>(
        string eventProviderType,
        string[] eventNames) =>
        new()
        {
            Events = eventNames,
            SendHandler = async (serviceProvider, eventName, message) =>
                await SendExternalEventAsync(serviceProvider: serviceProvider,eventProviderType: eventProviderType,eventName: eventName,message: message),
        };

    private static EventProvider<App> CreateAppDeleteExternalSendProvider() =>
        new()
        {
            Events = ["app_delete"],
            SendHandler = async (serviceProvider, eventName, message) =>
            {
                await SendExternalEventAsync(serviceProvider: serviceProvider,eventProviderType: "Http",eventName: eventName,message: message);

                if (message.Data is null)
                {
                    return;
                }

                await WaitForHostedServicesAppDeleteAsync(
serviceProvider:                     serviceProvider,appId:                     message.Data.Id);
            }
        };

    private static async ValueTask SendExternalEventAsync<T>(
        IServiceProvider serviceProvider,
        string eventProviderType,
        string eventName,
        EventMessage<T> message)
    {
        if (IsServiceBusEventProvider(eventProviderType: eventProviderType))
        {
            IAzureServiceBusEventHub serviceBusEventHub =
                serviceProvider.GetRequiredService<IAzureServiceBusEventHub>();

            await serviceBusEventHub.RaiseEventAsync(
name:                 eventName,message:                 new ServiceBusEventMessage<T>
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
        await httpEventHub.RaiseEventAsync(name: eventName,message: message);
    }

    private static async ValueTask WaitForHostedServicesAppDeleteAsync(
        IServiceProvider serviceProvider,
        int appId)
    {
        ICoreContextFactory contextFactory =
            serviceProvider.GetRequiredService<ICoreContextFactory>();

        for (int attempt = 0; attempt < 60; attempt++)
        {
            await using CoreDataContext core = contextFactory.CreateCoreContext();

            if (!await HasAppChildrenAsync(core: core,appId: appId))
            {
                return;
            }

            await Task.Delay(millisecondsDelay: 500);
        }

        throw new TimeoutException(
            $"Timed out waiting for Hosted Services to delete app {appId} children.");
    }

    private static async ValueTask<bool> HasAppChildrenAsync(
        CoreDataContext core,
        int appId) =>
        await core.Set<Role>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: role => role.AppId == appId)
        || await core.Set<AppCulture>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: culture => culture.AppId == appId)
        || await core.Set<Folder>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: folder => folder.AppId == appId)
        || await core.Set<MailServer>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: server => server.AppId == appId)
        || await core.Set<Calendar>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: calendar => calendar.AppId == appId)
        || await core.Set<FlowDefinition>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: flow => flow.AppId == appId);

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