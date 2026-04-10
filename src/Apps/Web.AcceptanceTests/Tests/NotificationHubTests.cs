using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

[Collection(WebAcceptanceCollection.Name)]
public sealed partial class NotificationHubTests(WebAcceptanceFixture fixture)
{
    private HttpClient Client { get; } = fixture.Client;
    private const string HubRoute = "/Api/Hubs/Notification";
    private const string Thread = "acceptance-notification";

    private async Task<HubConnection> ConnectAsync()
    {
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(new Uri(Client.BaseAddress!, HubRoute), options =>
            {
                options.HttpMessageHandlerFactory = _ => fixture.Factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        await connection.StartAsync().WaitAsync(TimeSpan.FromSeconds(10));
        return connection;
    }

    private async Task<int> NegotiateAsync()
    {
        using HttpRequestMessage request = new(
            HttpMethod.Post,
            $"{HubRoute}/negotiate?negotiateVersion=1"
        );
        using HttpResponseMessage response = await Client.SendAsync(request);
        return (int)response.StatusCode;
    }
}



