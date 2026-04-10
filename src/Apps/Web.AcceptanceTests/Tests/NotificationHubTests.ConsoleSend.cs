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
            connection.On<string, string, string>("ConsoleReceive", (level, message, receivedThread) =>
            {
                if (message == expectedMessage)
                    messageReceived.TrySetResult((level, message, receivedThread));
            });

            // When
            await connection.InvokeAsync("Join", Thread).WaitAsync(TimeSpan.FromSeconds(10));
            await connection
                .InvokeAsync("ConsoleSend", "info", expectedMessage, Thread)
                .WaitAsync(TimeSpan.FromSeconds(10));
            (string level, string message, string receivedThread) actual = await messageReceived
                .Task.WaitAsync(TimeSpan.FromSeconds(10));

            // Then
            actual.level.Should().Be("info");
            actual.message.Should().Be(expectedMessage);
            actual.receivedThread.Should().Be(Thread);
        }
        finally
        {
            await connection.StopAsync().WaitAsync(TimeSpan.FromSeconds(5));
            await connection.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
        }
    }
}



