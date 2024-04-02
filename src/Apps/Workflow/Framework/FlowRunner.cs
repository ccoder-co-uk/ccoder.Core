using Core.Objects.Dtos.Workflow;
using Core.Objects.Entities.Workflow;
using Core.Objects.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace Workflow.Framework
{
    public class FlowRunner
    {
        HubConnection con;

        public async Task Run(WorkflowRequest msg)
        {
            await ConnectToHub(msg);

            try
            {
                await Log(WorkflowLogLevel.Info, "Request received by workflow, processing ...", msg.InstanceId);
                await Log(WorkflowLogLevel.Debug, msg.ToJson(ObjectExtensions.GetJSONSettings(), Formatting.Indented), msg.InstanceId);

                // construct the flow instance
                Task log(WorkflowLogLevel level, string message) => Log(level, message, msg.InstanceId);
                var instance = await FlowHelper.GetInstance(msg.InstanceId, msg.Api, msg.AuthToken, log);

                // execute the flow 
                await ExecuteRequest(msg, instance);
            }
            catch (Exception ex)
            {
                await Log(WorkflowLogLevel.Fatal, $"Failed to process request, abandoning execution\n{ex.Message}\n{ex.StackTrace}", msg.InstanceId);
            }
            finally
            {
                await Log(WorkflowLogLevel.Info, "Done!", msg.InstanceId);
            }
        }

        async Task ConnectToHub(WorkflowRequest msg)
        {
            try
            {
                // connect to the signalr hub on the source API
                con = new HubConnectionBuilder().WithUrl(msg.Api + "Hubs/Workflow").Build();
                _ = con.On<Exception>("error", (ex) => Console.WriteLine(ex.Message + "\n" + ex.StackTrace));
                await con.StartAsync();
                await Log(WorkflowLogLevel.Info, $"Workflow Instance {msg.InstanceId} Connected.", msg.InstanceId);
            }
            catch (Exception ex)
            {
                await con.DisposeAsync();
                con = null;
                await Log(WorkflowLogLevel.Error, "Failed to connect to the hub because: " + ex.Message, msg.InstanceId);
            }
        }

        static async Task ExecuteRequest(WorkflowRequest msg, FlowInstance instance)
        {
            FlowInstanceData result = await (Task<FlowInstanceData>)instance
                .GetType()
                .GetMethod("Execute")
                .Invoke(instance, new[] { msg });

            await FlowHelper.SaveResult(result, msg.Api, msg.AuthToken);
        }

        public async Task Log(WorkflowLogLevel level, string message, Guid instanceId)
        {
            if (message.Length > 4000)
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
    }
}