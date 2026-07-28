// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Core.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Http;
using cCoder.Eventing.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Web;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder =
            WebApplication.CreateBuilder(args: args);

        builder.Services.AddWeb(
            applicationConfiguration: builder.Configuration,
            configure: configuration =>
                configuration.Eventing.EventProviders =
                    CreateEventProviders(configuration));

        WebApplication app = builder.Build();
        app.StartCoreWeb();
        app.MapHealthChecks(
            pattern: "/Health",
            options: new HealthCheckOptions
            {
                ResponseWriter = async (context, _) =>
                    await context.Response.WriteAsync(text: "OK")
            });
        app.Run();
    }

    private static EventProvider[] CreateEventProviders(
        CoreConfiguration configuration) =>
        configuration.Eventing.ProviderType?.Trim().ToUpperInvariant() switch
        {
            "HTTP" when !string.IsNullOrWhiteSpace(
                configuration.Eventing.Http.HubUrl) =>
                    CreateHttpEventProviders(),
            "SERVICEBUS" when !string.IsNullOrWhiteSpace(
                configuration.Eventing.ServiceBus.ConnectionString) =>
                    CreateServiceBusEventProviders(),
            _ => []
        };

    private static EventProvider[] CreateHttpEventProviders() =>
        [
            CreateHttpEventProvider<App>(
                ["app_add", "app_update", "app_delete"]),
            CreateHttpEventProvider<Folder>(["folder_delete"]),
            CreateHttpEventProvider<ScheduledTask>(
                ["scheduled_task_execute"]),
            CreateHttpEventProvider<FlowInstanceData>(
                ["flow_instance_data_add"])
        ];

    private static EventProvider[] CreateServiceBusEventProviders() =>
        [
            CreateServiceBusEventProvider<App>(["app_add", "app_update"]),
            CreateServiceBusEventProvider<ScheduledTask>(
                ["scheduled_task_execute"]),
            CreateServiceBusEventProvider<FlowInstanceData>(
                ["flow_instance_data_add"])
        ];

    private static EventProvider<T> CreateHttpEventProvider<T>(
        string[] eventNames) =>
        new()
        {
            Events = eventNames,
            SendHandler = async (serviceProvider, eventName, message) =>
            {
                IHttpEventHub eventHub =
                    serviceProvider.GetRequiredService<IHttpEventHub>();

                await eventHub.RaiseEventAsync(eventName, message);
            }
        };

    private static EventProvider<T> CreateServiceBusEventProvider<T>(
        string[] eventNames) =>
        new()
        {
            Events = eventNames,
            SendHandler = async (serviceProvider, eventName, message) =>
            {
                IAzureServiceBusEventHub eventHub =
                    serviceProvider
                        .GetRequiredService<IAzureServiceBusEventHub>();

                await eventHub.RaiseEventAsync(
                    eventName,
                    new ServiceBusEventMessage<T>
                    {
                        AuthInfo = new ServiceBusEventAuthInfo
                        {
                            SSOUserId =
                                message.AuthInfo?.SSOUserId ?? string.Empty
                        },
                        Data = message.Data
                    });
            }
        };
}