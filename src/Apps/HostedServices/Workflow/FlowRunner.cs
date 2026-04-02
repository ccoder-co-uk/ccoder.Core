using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Workflow.Framework;

public class FlowRunner
{
    private HubConnection con;

    public async Task Run(WorkflowRequest msg)
    {
        string lastStage = "initialising";
        bool completed = false;

        await ConnectToHub(msg);

        try
        {
            lastStage = "request-received";
            await Log(WorkflowLogLevel.Info, "Request received by workflow, processing ...", msg.InstanceId);
            await Log(WorkflowLogLevel.Debug, msg.ToJson(ObjectExtensions.GetJSONSettings(), Formatting.Indented), msg.InstanceId);

            // construct the flow instance
            FlowInstance instance = new((WorkflowLogLevel level, string message) => Log(level, message, msg.InstanceId));

            // execute the flow 
            lastStage = "executing-instance";
            await ExecuteRequest(msg, instance);
            completed = true;
        }
        catch (Exception ex)
        {
            await Log(
                WorkflowLogLevel.Fatal,
                $"Failed to process request during stage '{lastStage}', abandoning execution\n{ex.Message}\n{ex.StackTrace}",
                msg.InstanceId);
        }
        finally
        {
            string finalMessage = completed
                ? $"Workflow runner completed successfully. Final stage: {lastStage}"
                : $"Workflow runner ended without a successful completion. Last stage: {lastStage}";

            await Log(WorkflowLogLevel.Info, finalMessage, msg.InstanceId);
        }
    }

    private async Task ConnectToHub(WorkflowRequest msg)
    {
        try
        {
            // connect to the signalr hub on the source API
            con = new HubConnectionBuilder().WithUrl(msg.Api + "Hubs/Workflow", (opts) =>
            {
                opts.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                        clientHandler.ServerCertificateCustomValidationCallback += CertChainValidator.ValidateCertChain;

                    return message;
                };
            }).Build();

            con.On<Exception>("error", (ex) => Console.WriteLine(ex.Message + "\n" + ex.StackTrace));

            await con.StartAsync();
            await Log(WorkflowLogLevel.Info, $"Workflow Instance {msg.InstanceId} Connected.", msg.InstanceId);
        }
        catch (Exception ex)
        {
            if (con != null)
                await con.DisposeAsync();

            con = null;
            await Log(WorkflowLogLevel.Error, "Failed to connect to the hub because: " + ex.Message, msg.InstanceId);
        }
    }

    private async Task ExecuteRequest(WorkflowRequest msg, FlowInstance instance)
    {
        await Log(WorkflowLogLevel.Info, "Beginning flow instance execution.", msg.InstanceId);

        FlowInstanceData result = await (Task<FlowInstanceData>)instance
            .GetType()
            .GetMethod("Execute")
            .Invoke(instance, new[] { msg });

        int contextBytes = result.ContextJson?.Length ?? 0;
        await Log(
            WorkflowLogLevel.Info,
            $"Flow execution finished with state '{result.State ?? "<null>"}'. Persisting result ({contextBytes} bytes).",
            msg.InstanceId);

        await SaveResult(result, msg.Api, msg.AuthToken);

        await Log(WorkflowLogLevel.Info, "Workflow result persisted successfully.", msg.InstanceId);
    }

    public async Task Log(WorkflowLogLevel level, string message, Guid instanceId)
    {
        if (message.Length > 4000 && !message.Contains("Failed to deserialise"))
            message = $"{message[..1900]} ... {message.Length - 1900} characters cut due to excessive length.";

        Console.WriteLine($"{level}:: {message}");

        try
        {
            if (con != null)
                await con.InvokeAsync("ConsoleSend", level.ToString().ToLower(), message, instanceId.ToString());
        }
        catch (Exception ex)
        {
            if (con != null)
                await con.DisposeAsync();

            con = null;
            await Log(WorkflowLogLevel.Error, ex.Message, instanceId);
            await Log(level, message, instanceId);
        }

        Console.ForegroundColor = ConsoleColor.White;
    }

    private async Task SaveResult(FlowInstanceData result, string apiRoot, string auth)
    {
        using HttpClient api = Api(apiRoot);
        api.WithAuthToken(auth);
        api.Timeout = TimeSpan.FromSeconds(30);

        await Log(
            WorkflowLogLevel.Debug,
            $"PUT {apiRoot}Core/FlowInstanceData({result.Id}) state={result.State ?? "<null>"} contextBytes={result.ContextJson?.Length ?? 0}",
            result.Id);

        HttpResponseMessage response = await api.PutAsync(
            $"Core/FlowInstanceData({result.Id})",
            new StringContent(result.ToJsonForOdata(),
            Encoding.UTF8,
            "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            await Log(
                WorkflowLogLevel.Error,
                $"Failed to persist workflow result. HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n{body}",
                result.Id);
        }

        response.EnsureSuccessStatusCode();
    }

    private static HttpClient Api(string apiBase) => new(new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        ServerCertificateCustomValidationCallback = CertChainValidator.ValidateCertChain
    })
    { BaseAddress = new Uri(apiBase) };
}
