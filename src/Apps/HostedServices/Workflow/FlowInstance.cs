using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Objects.Workflow.Activities;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace Workflow.Framework;

public sealed class FlowInstance
{
    public Guid Id { get; private set; }

    public int AppId { get; private set; }

    public string Caller { get; set; }
    public string Name { get; private set; }
    public string FlowDefinition { get; private set; }

    internal Flow Flow { get; private set; }
    public WorkflowContext Context { get; private set; }


    public DateTimeOffset Start { get; private set; }
    public DateTimeOffset End { get; private set; }

    public event LogEvent OnLog;
    internal IScriptRunner Script { get; private set; }

    public FlowInstance(LogEvent log)
    {
        Script = new ScriptRunner(log);
        OnLog += log;
    }

    public async Task Log(WorkflowLogLevel level, string message)
    {
        try
        {
            await OnLog?.Invoke(level, message);
        }
        catch (Exception ex)
        {
            OnLog = null;
            Context.Log(WorkflowLogLevel.Warning, "Realtime processing has failed:\n" + ex.Message + "\nLog streaming has been disabled.");
        }
    }

    public async Task<FlowInstanceData> Execute(WorkflowRequest request)
    {
        Start = DateTimeOffset.UtcNow;

        await Log(WorkflowLogLevel.Info, $"Fetching persisted workflow instance {request.InstanceId}.");
        using HttpClient api = Api(request.Api).WithAuthToken(request.AuthToken);
        string rawInstance = await api.GetStringAsync($"Core/FlowInstanceData({request.InstanceId})?$expand=FlowDefinition($select=Id,AppId)");
        await Log(WorkflowLogLevel.Debug, $"Fetched workflow instance payload ({System.Text.Encoding.UTF8.GetByteCount(rawInstance)} bytes).");
        FlowInstanceData instanceData = await DeserialiseInstance(rawInstance);
        await Log(WorkflowLogLevel.Info, $"Deserialised workflow instance {instanceData.Id}.");

        AppId = instanceData.FlowDefinition.AppId;
        Id = instanceData.Id;
        Name = instanceData.Name;
        FlowDefinition = instanceData.FlowDefinition.ToJson();

        await Log(WorkflowLogLevel.Debug, $"Deserialising workflow context ({instanceData.ContextJson?.Length ?? 0} bytes).");
        cCoder.Core.Objects.Dtos.Workflow.WorkflowContext dtoContext = await DeserialiseContext(instanceData.ContextString);
        Flow = dtoContext.Flow;
        await Log(WorkflowLogLevel.Info, $"Workflow context deserialised with {Flow?.Activities?.Length ?? 0} activities and {Flow?.Links?.Length ?? 0} links.");

        await Stitch();
        await Log(WorkflowLogLevel.Info, "Workflow graph stitched successfully.");

        Context = new WorkflowContext(Flow, this);
        await Log(WorkflowLogLevel.Info, "Starting workflow context execution.");
        await Context.Execute(request.Api, request.AuthToken);
        await Log(WorkflowLogLevel.Info, $"Workflow context execution finished with state '{Context.ExecutionState ?? "<null>"}'.");
        return Complete();
    }

    private async Task<FlowInstanceData> DeserialiseInstance(string rawInstance)
    {
        try
        {
            return JsonConvert.DeserializeObject<FlowInstanceData>(rawInstance, ObjectExtensions.GetJSONSettings());
        }
        catch
        {
            await Log(WorkflowLogLevel.Error, $"Failed to deserialise flow instance: \n{rawInstance}");
            throw;
        }
    }

    private async Task<cCoder.Core.Objects.Dtos.Workflow.WorkflowContext> DeserialiseContext(string rawContext)
    {
        try
        {
            return JsonConvert.DeserializeObject<cCoder.Core.Objects.Dtos.Workflow.WorkflowContext>(rawContext, ObjectExtensions.GetJSONSettings());
        }
        catch
        {
            await Log(WorkflowLogLevel.Error, $"Failed to deserialise flow context: \n{rawContext}");
            throw;
        }
    }

    private FlowInstanceData Complete()
    {
        FlowDefinition flowDef = JsonConvert.DeserializeObject<FlowDefinition>(FlowDefinition, ObjectExtensions.GetJSONSettings());

        Context.Flow.Activities.ForEach((a) =>
        {
            PropertyInfo[] props = a.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttribute<IgnoreWhenFlowCompleteAttribute>() != null)
                .ToArray();

            props.ForEach((p) => p.SetValue(a, default));
        });

        return new FlowInstanceData
        {
            Id = Id,
            Name = Name,
            Caller = Caller,
            FlowDefinitionId = flowDef.Id,
            ContextString = Context.ToJson(),
            State = Context.ExecutionState,
            Start = Start,
            End = DateTimeOffset.UtcNow
        };
    }

    private async Task Stitch()
    {
        foreach (Activity activity in Flow.Activities)
        {
            // link previous 
            try
            {
                string[] links = Flow.Links.Where(l => l.Destination == activity.Ref).Select(l => l.Source).ToArray();
                activity.Previous = Flow.Activities.Where(a => links.Contains(a.Ref)).ToArray();
            }
            catch (Exception ex)
            {
                await Log(WorkflowLogLevel.Error, $"Problem in previous activity selection for activity {activity.Ref}:\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        foreach (Activity activity in Flow.Activities)
        {

            // link next
            try
            {
                activity.Next = Flow.Activities.Where(a => a.Previous?.Contains(activity) ?? false).ToArray();
            }
            catch (Exception ex)
            {
                await Log(WorkflowLogLevel.Error, $"Problem in next activity selection for activity {activity.Ref}:\n{ex.Message}\n{ex.StackTrace}");
            }

            // compile link code
            try
            {
                activity.AssignCode = BuildAssign(activity, Flow);
            }
            catch (Exception ex)
            {
                await Log(WorkflowLogLevel.Error, $"Problem in one or more links for activity {activity.Ref}:\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    private static string BuildAssign(Activity activity, Flow flow)
    {
        string[] assigns = activity.Previous.Select(source =>
        {
            Link link = flow.Links.First(l => l.Source == source.Ref && l.Destination == activity.Ref);
            string sourceType = source.GetType().GetCSharpTypeName();
            string destType = activity.GetType().GetCSharpTypeName();
            return string.IsNullOrEmpty(link.Expression?.Trim())
                ? null
                : $"//LINK:: {source.Ref} => {activity.Ref}\n" + link.Expression
                    .Replace("destination.", $"(({destType})activity).")
                    .Replace("source.", $"flow.GetActivity<{sourceType}>(\"{source.Ref}\").");
        })
            .Where(i => i != null)
            .ToArray();

        string body = $"\t{string.Join(";\n\t", assigns)}";

        // the complete block as a single Action that can be called
        return assigns.Length != 0
            ? $"(activity, variables, flow) => {{\n{body}\n}}"
            : null;
    }

    internal HttpClient Api(string apiBase) => new(new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        ServerCertificateCustomValidationCallback = CertChainValidator.ValidateCertChain
    })
    { BaseAddress = new Uri(apiBase) };
}
