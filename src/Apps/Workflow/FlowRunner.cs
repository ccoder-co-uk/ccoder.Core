using System.Net;
using System.Text;
using cCoder.Data.Models.Workflow;
using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Activities.Support;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace Workflow;

public sealed class FlowRunner
{
    private HubConnection connection;

    public async Task RunAsync(WorkflowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        await ConnectToHubAsync(request);

        try
        {
            await LogAsync(WorkflowLogLevel.Info, "Request received by workflow, processing ...", request.InstanceId);
            await LogAsync(WorkflowLogLevel.Debug, request.ToJson(), request.InstanceId);

            FlowInstance instance = new((level, message) => LogAsync(level, message, request.InstanceId));
            FlowInstanceData result = await instance.ExecuteAsync(request);
            await SaveResultAsync(result, request.Api, request.AuthToken);
        }
        catch (Exception exception)
        {
            await LogAsync(
                WorkflowLogLevel.Fatal,
                $"Failed to process request, abandoning execution{Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}",
                request.InstanceId);
            throw;
        }
        finally
        {
            await LogAsync(WorkflowLogLevel.Info, "Done!", request.InstanceId);
        }
    }

    public async Task LogAsync(WorkflowLogLevel level, string message, Guid instanceId)
    {
        if (message?.Length > 4000 && !message.Contains("Failed to deserialise", StringComparison.OrdinalIgnoreCase))
            message = $"{message[..1900]} ... {message.Length - 1900} characters cut due to excessive length.";

        Console.WriteLine($"{level}:: {message}");

        try
        {
            if (connection is not null)
                await connection.InvokeAsync("ConsoleSend", level.ToString().ToLowerInvariant(), message, instanceId.ToString());
        }
        catch (Exception exception)
        {
            if (connection is not null)
                await connection.DisposeAsync();

            connection = null;
            await LogAsync(WorkflowLogLevel.Error, exception.Message, instanceId);
            await LogAsync(level, message, instanceId);
        }
        finally
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    private async Task ConnectToHubAsync(WorkflowRequest request)
    {
        try
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{request.Api}Hubs/Workflow", options =>
                {
                    options.HttpMessageHandlerFactory = handler =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                            clientHandler.ServerCertificateCustomValidationCallback += CertChainValidator.ValidateCertChain;

                        return handler;
                    };
                })
                .Build();

            connection.On<Exception>("error", exception => Console.WriteLine($"{exception.Message}{Environment.NewLine}{exception.StackTrace}"));

            await connection.StartAsync();
            await LogAsync(WorkflowLogLevel.Info, $"Workflow instance {request.InstanceId} connected.", request.InstanceId);
        }
        catch (Exception exception)
        {
            if (connection is not null)
                await connection.DisposeAsync();

            connection = null;
            await LogAsync(WorkflowLogLevel.Warning, $"Workflow hub connection could not be established: {exception.Message}", request.InstanceId);
        }
    }

    private static async Task SaveResultAsync(FlowInstanceData result, string apiRoot, string authToken)
    {
        using HttpClient api = CreateApiClient(apiRoot);
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
        string payload = JsonConvert.SerializeObject(
            new
            {
                result.Id,
                result.FlowDefinitionId,
                result.Name,
                result.State,
                result.ReportingComponentName,
                result.Caller,
                ContextString = result.ContextString,
                result.Start,
                result.End
            },
            Formatting.None);

        HttpResponseMessage response = await api.PutAsync(
            $"Core/FlowInstanceData({result.Id})",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Workflow result save failed with status {(int)response.StatusCode} ({response.StatusCode})."
                + $"{Environment.NewLine}Payload:{Environment.NewLine}{payload}"
                + $"{Environment.NewLine}Response:{Environment.NewLine}{responseBody}");
        }

        response.EnsureSuccessStatusCode();
    }

    private static HttpClient CreateApiClient(string apiRoot) => new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        ServerCertificateCustomValidationCallback = CertChainValidator.ValidateCertChain
    })
    {
        BaseAddress = new Uri(apiRoot)
    };
}
