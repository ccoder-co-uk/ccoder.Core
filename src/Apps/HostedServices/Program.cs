// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Core.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing;
using cCoder.Eventing.Models;
using cCoder.Security.Models.Events;
using cCoder.Workflow.Activities.Models;
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
                    CreateEventProviders(configuration));

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

    private static EventProvider[] CreateEventProviders(
        CoreConfiguration configuration) =>
        [
            CreateExternalReceiveProvider<App>(
                ["app_add", "app_update", "app_delete"]),
            CreateExternalReceiveProvider<Folder>(["folder_delete"]),
            CreateExternalReceiveProvider<ScheduledTask>(
                ["scheduled_task_execute"]),
            CreateQueuedFlowInstanceReceiveProvider(),
            CreateWorkflowExecuteProvider(configuration),
            CreateExternalReceiveProvider<SecurityAccountEvent>(
                CreateSecurityAccountEventNames()),
        ];

    private static EventProvider<WorkflowRequest> CreateWorkflowExecuteProvider(
        CoreConfiguration configuration) =>
        new()
        {
            Events = ["workflow_execute"],
            SendHandler = async (_, _, message) =>
            {
                using HttpClient client = new()
                {
                    BaseAddress = new Uri(configuration.Workflow.ServiceUrl)
                };

                using HttpResponseMessage response =
                    await client.PostAsJsonAsync(
                        requestUri: "Execute",
                        value: message.Data);

                response.EnsureSuccessStatusCode();
            }
        };

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
