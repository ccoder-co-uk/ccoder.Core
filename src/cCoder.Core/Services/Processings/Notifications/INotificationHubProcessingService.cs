// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Services.Processings.Notifications;

public interface INotificationHubProcessingService
{
    Task OnConnectedAsync(Func<Task> onConnectedAsync);
    Task OnDisconnectedAsync(
        Exception exception,
        Func<Exception, Task> onDisconnectedAsync);
    Task JoinAsync(
        string thread,
        string connectionId,
        IGroupManager groups,
        IHubCallerClients clients);
    Task LeaveAsync(
        string thread,
        string connectionId,
        IGroupManager groups,
        IHubCallerClients clients);
    void Send(
        string level,
        string message,
        string thread,
        IHubCallerClients clients);
    Task ConsoleSendAsync(
        string level,
        string message,
        string thread,
        IHubCallerClients clients);
    Task SendTestAsync(
        string message,
        string thread,
        IHubCallerClients clients);
}