using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Objects.Workflow.Activities;
using Newtonsoft.Json;

namespace Workflow.Framework;

/// <summary>
/// Construct one of these to execute a task structure.
/// The execution context is passed between tasks as they execute.
/// It also acts as a utility class for storing process related data across tasks in a single execution
/// </summary>
public sealed class WorkflowContext : cCoder.Core.Objects.Dtos.Workflow.WorkflowContext, IWorkflowContext
{
    [JsonIgnore]
    public IScriptRunner Script => Instance?.Script ?? new ScriptRunner((l, m) => Task.Run(() => { Log(l, m); }));

    [JsonIgnore]
    internal FlowInstance Instance { get; set; }


    public WorkflowContext()
    {
        ExecutionLog = new List<WorkflowLogEntry>();
        Variables = new Dictionary<string, object>()
        {
            // Default script imports, override in flow to change this 
            { "Imports", Activity.ScriptImports }
        };
    }

    internal WorkflowContext(Flow flow, FlowInstance instance) : this()
    {
        Flow = flow;
        ExecutionLog = new List<WorkflowLogEntry>();
        InstanceId = instance.Id;
        Instance = instance;
    }

    public async Task Execute(string apiRoot, string authToken = null)
    {
        try
        {
            //await CompileAllLinks();

            Log(WorkflowLogLevel.Info, "Execution started");
            Variables.Add("AppId", Instance.AppId);
            Variables.Add("Api", apiRoot);
            Variables.Add("AuthToken", authToken);
            Variables.Add("InstanceId", InstanceId.ToString());

            HttpClient api = Instance.Api(apiRoot).WithAuthToken(authToken);

            using (api)
            {
                User userDetails = await api.GetAsync<User>("Core/User/Me()");
                Instance.Caller = userDetails.Id;
                Variables.Add("UserId", userDetails.Id);
                Variables.Add("UserName", userDetails.DisplayName);
                Variables.Add("UserEmail", userDetails.Email);
            }

            Start start = (Start)Flow.Activities.First(a => a is Start);

            if (authToken != null)
                start.AuthToken = authToken;

            if (Variables.TryGetValue("Data", out object value))
                start.Data = value;

            await start.ExecuteInternal(this);
        }
        catch (Exception ex)
        {
            // log the failure
            Log(WorkflowLogLevel.Error, "Execution failed.");
            Log(WorkflowLogLevel.Error, ex.Message + "\n" + ex.StackTrace);

            Exception e = ex.InnerException;
            while (e != null)
            {
                Log(WorkflowLogLevel.Error, ex.Message + "\n" + ex.StackTrace);
                e = e.InnerException;
            }
        }

        EvaluateFinalState();
    }

    private async Task CompileAllLinks()
    {
        List<string> items = new();

        foreach (Activity activity in Flow.Activities)
            items.Add($"{{ flow.Activities.First(a => a.Ref == \"{activity.Ref}\"), {activity.AssignCode ?? "(activity, variables, flow) => { }"} }}");

        string script = $"(Flow flow) => {{ var result = new Dictionary<Activity, Action<Activity, IDictionary<string, object>, Flow>> {{ {string.Join(",\n", items)} }};\n return result; }}";

        Func<Flow, IDictionary<Activity, Action<Activity, IDictionary<string, object>, Flow>>> buildLinks = await Script.BuildScript<Func<Flow, IDictionary<Activity, Action<Activity, IDictionary<string, object>, Flow>>>>(
            code: script,
            imports: (string[])Variables["Imports"] ?? Activity.ScriptImports,
            log: Log);

        Activity.CompiledLinkCache = buildLinks(Flow);
    }

    public void Log(WorkflowLogLevel level, string message)
    {
        ExecutionLog.Add(new WorkflowLogEntry(level, message));
        Instance?.Log(level, message).Wait();
    }

    private void EvaluateFinalState()
    {
        if (Flow.Activities.All(a => a.State is ActivityState.Complete or ActivityState.Skipped))
        {
            Log(WorkflowLogLevel.Info, "Execution Complete.");
            ExecutionState = "Complete";

            if (ExecutionLog.Any(i => i.Level == "Warn"))
                ExecutionState = "CompletedWithWarnings";
        }
        else
            ExecutionState = "Failed";
    }
}