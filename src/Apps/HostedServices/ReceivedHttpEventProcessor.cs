using System.Text.Json;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Eventing;
using cCoder.Eventing.Http.Models;
using cCoder.Eventing.Models;

namespace HostedServices;

public sealed class ReceivedHttpEventProcessor(
    IEventHub eventHub,
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
            options.JsonSerializerOptions);

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
}
