// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Brokers.Notifications;
using cCoder.Core.Exposures.Hubs;
using cCoder.Core.Services.Processings.Notifications;
using cCoder.Core.Services.Foundations.Notifications;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace cCoder.Core.Tests.Exposures.Hubs;

public sealed partial class NotificationHubBehaviorTests
{
    [Fact]
    public async Task Join_WhenThreadIsNew_AddsConnectionAndNotifiesCallerAndGroup()
    {
        // Given
        string thread = $"notification-{Guid.NewGuid():N}";
        HubTestContext context = new();

        // When
        await context.Hub.Join(thread: thread);

        // Then
        context.GroupsMock.Verify(
            expression: groups => groups.AddToGroupAsync(
                connectionId: context.ConnectionId,
                groupName: thread,
                cancellationToken: It.IsAny<CancellationToken>()),
            times: Times.Once);

        VerifySingleClientMessage(
            clientMock: context.CallerMock,
            method: "ConsoleReceive",
            arguments: ["info", $"Connected to instance {thread}", thread]);

        VerifyMessage(
            clientMock: context.GroupMock,
            method: "ConsoleReceive",
            arguments: ["info", "User Joined", thread]);
    }

    [Fact]
    public async Task Join_WhenThreadHasHistory_ReplaysHistoryToCaller()
    {
        // Given
        string thread = $"notification-{Guid.NewGuid():N}";
        HubTestContext context = new();

        await context.Hub.ConsoleSend(
            level: "warning",
            message: "Earlier message",
            thread: thread);

        // When
        await context.Hub.Join(thread: thread);

        // Then
        VerifySingleClientMessage(
            clientMock: context.CallerMock,
            method: "ConsoleReceive",
            arguments: ["warning", "Earlier message", thread]);
    }

    [Fact]
    public async Task Leave_WhenLastConnectionLeaves_RemovesConnectionAndNotifiesCallerAndGroup()
    {
        // Given
        string thread = $"notification-{Guid.NewGuid():N}";
        HubTestContext context = new();
        await context.Hub.Join(thread: thread);

        // When
        await context.Hub.Leave(thread: thread);

        // Then
        context.GroupsMock.Verify(
            expression: groups => groups.RemoveFromGroupAsync(
                connectionId: context.ConnectionId,
                groupName: thread,
                cancellationToken: It.IsAny<CancellationToken>()),
            times: Times.Once);

        VerifySingleClientMessage(
            clientMock: context.CallerMock,
            method: "info",
            arguments: [$"Stopped listening to messages for {thread}", thread]);

        VerifyMessage(
            clientMock: context.GroupMock,
            method: "ConsoleReceive",
            arguments: ["info", "User Left", thread]);
    }

    [Fact]
    public async Task ConsoleSend_WhenMessageIsSent_BroadcastsItToThread()
    {
        // Given
        string thread = $"notification-{Guid.NewGuid():N}";
        HubTestContext context = new();

        // When
        await context.Hub.ConsoleSend(
            level: "info",
            message: "Current message",
            thread: thread);

        // Then
        VerifyMessage(
            clientMock: context.GroupMock,
            method: "ConsoleReceive",
            arguments: ["info", "Current message", thread]);
    }

    [Fact]
    public async Task SendTest_WhenMessageIsSent_BroadcastsTestMessageToThread()
    {
        // Given
        string thread = $"notification-{Guid.NewGuid():N}";
        HubTestContext context = new();

        // When
        await context.Hub.SendTest(
            message: "Test message",
            thread: thread);

        // Then
        VerifyMessage(
            clientMock: context.GroupMock,
            method: "ConsoleReceive",
            arguments: ["test", "Test message", thread]);
    }

    private static void VerifyMessage(
        Mock<IClientProxy> clientMock,
        string method,
        object[] arguments) =>
        clientMock.Verify(
            expression: client => client.SendCoreAsync(
                method: method,
                args: It.Is<object[]>(match: actualArguments =>
                    actualArguments.SequenceEqual(second: arguments)),
                cancellationToken: It.IsAny<CancellationToken>()),
            times: Times.Once);

    private static void VerifySingleClientMessage(
        Mock<ISingleClientProxy> clientMock,
        string method,
        object[] arguments) =>
        clientMock.Verify(
            expression: client => client.SendCoreAsync(
                method: method,
                args: It.Is<object[]>(match: actualArguments =>
                    actualArguments.SequenceEqual(second: arguments)),
                cancellationToken: It.IsAny<CancellationToken>()),
            times: Times.Once);

    private sealed class HubTestContext
    {
        internal string ConnectionId { get; } = Guid.NewGuid()
            .ToString(format: "N");
        internal Mock<ISingleClientProxy> CallerMock { get; } = new();
        internal Mock<IClientProxy> GroupMock { get; } = new();
        internal Mock<IGroupManager> GroupsMock { get; } = new();
        internal NotificationHub Hub { get; }

        internal HubTestContext()
        {
            Mock<ILoggingBroker> loggingBrokerMock = new();
            Mock<HubCallerContext> callerContextMock = new();
            Mock<IHubCallerClients> clientsMock = new();

            callerContextMock
                .Setup(expression: context => context.ConnectionId)
                .Returns(value: ConnectionId);

            clientsMock
                .Setup(expression: clients => clients.Caller)
                .Returns(value: CallerMock.Object);

            clientsMock
                .Setup(expression: clients => clients.Group(groupName: It.IsAny<string>()))
                .Returns(value: GroupMock.Object);

            INotificationHubProcessingService processingService =
                new NotificationHubProcessingService(
                    log: loggingBrokerMock.Object,
                    notificationHubService: new NotificationHubService(
                        notificationHubBroker: new NotificationHubBroker()));

            Hub = new NotificationHub(
                notificationHubProcessingService: processingService)
            {
                Clients = clientsMock.Object,
                Context = callerContextMock.Object,
                Groups = GroupsMock.Object
            };
        }
    }
}