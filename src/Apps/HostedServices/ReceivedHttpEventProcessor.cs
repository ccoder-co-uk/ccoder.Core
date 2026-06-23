using System.Text.Json;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing;
using cCoder.Eventing.Http.Models;
using cCoder.Eventing.Models;
using cCoder.Workflow.Services.Orchestrations;

namespace HostedServices;

public sealed class ReceivedHttpEventProcessor(
    IEventHub eventHub,
    IWorkflowInstanceManagementOrchestrationService workflowInstanceManagementService,
    HttpEventingOptions options)
{
    public ValueTask ProcessAsync(
        HttpEventMessage message,
        CancellationToken cancellationToken = default) =>
            message?.EventName switch
            {
                "app_add" => RaiseAsync<App>(message, cancellationToken),
                "app_update" => RaiseAsync<App>(message, cancellationToken),
                "app_delete" => RaiseAsync<App>(message, cancellationToken),
                "folder_delete" => RaiseAsync<Folder>(message, cancellationToken),
                "flow_instance_data_add" => ExecuteWorkflowAsync(message, cancellationToken),
                null or "" => throw new InvalidOperationException(
                    "You must provide an event name when receiving events."),
                _ => throw new InvalidOperationException(
                    $"No synchronous HTTP event processor is registered for event '{message.EventName}'."),
            };

    private async ValueTask RaiseAsync<T>(
        HttpEventMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(message.Data))
        {
            throw new InvalidOperationException(
                "You must provide message data when receiving events.");
        }

        T data = JsonSerializer.Deserialize<T>(
            message.Data,
            options.JsonSerializerOptions)
            ?? throw new InvalidOperationException(
                $"You must provide a valid payload for event '{message.EventName}'.");

        await eventHub.RaiseEventAsync(
            message.EventName,
            new EventMessage<T>
            {
                AuthInfo = new EventAuthInfo
                {
                    SSOUserId = message.SSOUserId ?? "Guest",
                },
                Data = data,
            });
    }

    private async ValueTask ExecuteWorkflowAsync(
        HttpEventMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(message.Data))
        {
            throw new InvalidOperationException(
                "You must provide message data when receiving events.");
        }

        FlowInstanceData flowInstanceData = JsonSerializer.Deserialize<FlowInstanceData>(
            message.Data,
            options.JsonSerializerOptions)
            ?? throw new InvalidOperationException(
                "You must provide a valid workflow instance payload when receiving events.");

        if (flowInstanceData.FlowDefinitionId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "You must provide a workflow instance payload with a valid flow definition id.");
        }

        await workflowInstanceManagementService.ExecuteWaitingQueuedInstanceByIdAsync(
            flowInstanceData.FlowDefinitionId);
    }
}
