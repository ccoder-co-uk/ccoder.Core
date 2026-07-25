// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class NotificationHubTests
{
    [Fact]
    public async Task ShouldBroadcastToJoinedGroupWhenConsoleSend()
    {
        // Given
        string expectedMessage = $"acceptance-message-{Guid.NewGuid():N}";

        TaskCompletionSource<(string level, string message, string thread)> messageReceived =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        HubConnection connection = await ConnectAsync();

        try
        {
            connection.On<string, string, string>(methodName: "ConsoleReceive",handler: (level, message, receivedThread) =>
            {
                if (message == expectedMessage)
                {
                    messageReceived.TrySetResult(result: (level, message, receivedThread));
                }
            });

            // When
            await connection.InvokeAsync(methodName: "Join",arg1: Thread)
                .WaitAsync(timeout: TimeSpan.FromSeconds(seconds: 10));

            await connection
                .InvokeAsync(methodName: "ConsoleSend",arg1: "info",arg2: expectedMessage,arg3: Thread)
                .WaitAsync(timeout: TimeSpan.FromSeconds(seconds: 10));

            (string level, string message, string receivedThread) actual = await messageReceived
                .Task.WaitAsync(timeout: TimeSpan.FromSeconds(seconds: 10));

            // Then
            actual.level.Should()
                .Be(expected: "info");

            actual.message.Should()
                .Be(expected: expectedMessage);

            actual.receivedThread.Should()
                .Be(expected: Thread);
        }
        finally
        {
            await connection.StopAsync()
                .WaitAsync(timeout: TimeSpan.FromSeconds(seconds: 5));

            await connection.DisposeAsync()
                .AsTask()
                .WaitAsync(timeout: TimeSpan.FromSeconds(seconds: 5));
        }
    }
}