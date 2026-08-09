// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Core.Models;
using cCoder.ContentManagement.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing;
using cCoder.Eventing.Models;
using cCoder.Security.Models.Events;
using cCoder.Workflow.Exposures;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder =
            WebApplication.CreateBuilder(args: args);

        builder.Services.AddHostedServices(
            applicationConfiguration: builder.Configuration,
            configure: configuration =>
                configuration.Eventing.EventProviders =
                    CreateEventProviders());

        WebApplication app = builder.Build();
        app.StartCoreHostedServices();
        app.MapHealthChecks(
            pattern: "/Health",
            options: new HealthCheckOptions
            {
                ResponseWriter = async (context, _) =>
                    await context.Response.WriteAsync(text: "OK")
            });
        app.Run();
    }

    private static EventProvider[] CreateEventProviders() =>
        [
            CreateExternalReceiveProvider<App>(
                ["app_add", "app_update", "app_delete"]),
            CreateExternalReceiveProvider<Folder>(["folder_delete"]),
            CreateExternalReceiveProvider<ScheduledTask>(
                ["scheduled_task_execute"]),
            CreateQueuedFlowInstanceReceiveProvider(),
            CreateExternalReceiveProvider<PackageImportEvent>(
                ["package_import_complete"]),
            CreateExternalReceiveProvider<UncachedPageRenderEvent>(
                ["uncached_page_render"]),
            CreateExternalReceiveProvider<SecurityAccountEvent>(
                CreateSecurityAccountEventNames()),
        ];

    private static string[] CreateSecurityAccountEventNames() =>
        [
            SecurityAccountEventKind.RegistrationCreated.ToEventName(),
            SecurityAccountEventKind.InvitationCreated.ToEventName(),
            SecurityAccountEventKind.PasswordResetRequested.ToEventName()
        ];

    private static EventProvider<T> CreateExternalReceiveProvider<T>(
        string[] eventNames) =>
        new()
        {
            Events = eventNames,
            ReceiveHandler = async (
                serviceProvider,
                eventName,
                message) =>
            {
                IEventHub eventHub =
                    serviceProvider.GetRequiredService<IEventHub>();

                await eventHub.RaiseEventAsync(
                    eventName,
                    new EventMessage<T>
                    {
                        AuthInfo = new EventAuthInfo
                        {
                            SSOUserId =
                                message.AuthInfo?.SSOUserId ?? "Guest",
                        },
                        Data = message.Data,
                    });
            }
        };

    private static EventProvider<FlowInstanceData>
        CreateQueuedFlowInstanceReceiveProvider() =>
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

                if (!string.Equals(
                    message.Data?.State,
                    "Queued",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                IWorkflowInstanceManager processingService =
                    serviceProvider.GetRequiredService<
                        IWorkflowInstanceManager>();

                await processingService
                    .ExecuteWaitingQueuedInstanceByIdAsync(
                        message.Data.Id);
            }
        };
}