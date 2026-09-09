// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Exposures.Hubs;

public sealed class NotificationHub(
    INotificationHubProcessingService notificationHubProcessingService)
    : Hub
{
    public override Task OnConnectedAsync() =>
        notificationHubProcessingService.OnConnectedAsync(
            onConnectedAsync: base.OnConnectedAsync);

    public override Task OnDisconnectedAsync(Exception exception) =>
        notificationHubProcessingService.OnDisconnectedAsync(
            exception: exception,
            onDisconnectedAsync: base.OnDisconnectedAsync);

    public Task Join(string thread) =>
        notificationHubProcessingService.JoinAsync(
            thread: thread,
            connectionId: Context.ConnectionId,
            groups: Groups,
            clients: Clients);

    public Task Leave(string thread) =>
        notificationHubProcessingService.LeaveAsync(
            thread: thread,
            connectionId: Context.ConnectionId,
            groups: Groups,
            clients: Clients);

    public void Send(string level, string message, string thread) =>
        notificationHubProcessingService.Send(
            level: level,
            message: message,
            thread: thread,
            clients: Clients);

    public Task ConsoleSend(string level, string message, string thread) =>
        notificationHubProcessingService.ConsoleSendAsync(
            level: level,
            message: message,
            thread: thread,
            clients: Clients);

    public Task SendTest(string message, string thread) =>
        notificationHubProcessingService.SendTestAsync(
            message: message,
            thread: thread,
            clients: Clients);
}