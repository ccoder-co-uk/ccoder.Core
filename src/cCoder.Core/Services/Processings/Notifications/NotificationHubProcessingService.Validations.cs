// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Services.Processings.Notifications;

internal sealed partial class NotificationHubProcessingService
{
    private static void ValidateOnConnected(
        Func<Task> onConnectedAsync) =>
        ValidationRulesEngine.Validate(inputs: [onConnectedAsync]);

    private static void ValidateOnDisconnected(
        Exception exception,
        Func<Exception, Task> onDisconnectedAsync) =>
        ValidationRulesEngine.Validate(
            inputs: [onDisconnectedAsync]);

    private static void ValidateOnJoin(
        string thread,
        string connectionId,
        IGroupManager groups,
        IHubCallerClients clients) =>
        ValidationRulesEngine.Validate(
            inputs: [thread, connectionId, groups, clients]);

    private static void ValidateOnLeave(
        string thread,
        string connectionId,
        IGroupManager groups,
        IHubCallerClients clients) =>
        ValidationRulesEngine.Validate(
            inputs: [thread, connectionId, groups, clients]);

    private static void ValidateOnSend(
        string level,
        string message,
        string thread,
        IHubCallerClients clients) =>
        ValidationRulesEngine.Validate(
            inputs: [level, message, thread, clients]);

    private static void ValidateOnConsoleSend(
        string level,
        string message,
        string thread,
        IHubCallerClients clients) =>
        ValidationRulesEngine.Validate(
            inputs: [level, message, thread, clients]);

    private static void ValidateOnSendTest(
        string message,
        string thread,
        IHubCallerClients clients) =>
        ValidationRulesEngine.Validate(
            inputs: [message, thread, clients]);
}