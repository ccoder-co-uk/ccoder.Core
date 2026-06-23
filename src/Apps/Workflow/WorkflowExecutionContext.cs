using cCoder.Data.Models.Security;
using cCoder.Workflow.Activities;
using cCoder.Workflow.Activities.Activities;
using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Activities.Support;
using Newtonsoft.Json;

namespace Workflow;

public sealed class WorkflowExecutionContext : WorkflowContext, IWorkflowContext
{
    public WorkflowExecutionContext()
    {
        ExecutionLog = new List<WorkflowLogEntry>();
        Variables = new Dictionary<string, object>
        {
            ["Imports"] = Activity.ScriptImports
        };
    }

    public WorkflowExecutionContext(Flow flow, FlowInstance instance)
        : this()
    {
        Flow = flow;
        InstanceId = instance.Id;
        Instance = instance;
    }

    [JsonIgnore]
    public IScriptRunner Script => Instance?.Script ?? new ScriptRunner((level, message) =>
    {
        Log(level, message);
        return Task.CompletedTask;
    });

    [JsonIgnore]
    internal FlowInstance Instance { get; private set; }

    public async Task ExecuteAsync(string apiRoot, string authToken = null)
    {
        try
        {
            Log(WorkflowLogLevel.Info, "Execution started");
            Variables["AppId"] = Instance.AppId;
            Variables["Api"] = apiRoot;
            Variables["AuthToken"] = authToken;
            Variables["InstanceId"] = InstanceId.ToString();

            using HttpClient api = Instance.CreateApiClient(apiRoot);
            api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            User user = await api.GetAsync<User>("Core/User/Me()");
            Instance.Caller = user.Id;
            Variables["UserId"] = user.Id;
            Variables["UserName"] = user.DisplayName;
            Variables["UserEmail"] = user.Email;

            Start start = Flow.Activities.OfType<Start>().First();

            if (authToken is not null)
                start.AuthToken = authToken;

            if (Variables.TryGetValue("Data", out object data))
                start.Data = data;

            await start.ExecuteInternal(this);
        }
        catch (Exception exception)
        {
            Log(WorkflowLogLevel.Error, "Execution failed.");
            Log(WorkflowLogLevel.Error, $"{exception.Message}{Environment.NewLine}{exception.StackTrace}");

            Exception inner = exception.InnerException;
            while (inner is not null)
            {
                Log(WorkflowLogLevel.Error, $"{inner.Message}{Environment.NewLine}{inner.StackTrace}");
                inner = inner.InnerException;
            }
        }

        EvaluateFinalState();
    }

    public void Log(WorkflowLogLevel level, string message)
    {
        ExecutionLog.Add(new WorkflowLogEntry(level, message));
        Instance?.LogAsync(level, message).GetAwaiter().GetResult();
    }

    private void EvaluateFinalState()
    {
        if (Flow.Activities.All(activity => activity.State is ActivityState.Complete or ActivityState.Skipped))
        {
            Log(WorkflowLogLevel.Info, "Execution complete.");
            ExecutionState = ExecutionLog.Any(entry => entry.Level == "Warn")
                ? "CompletedWithWarnings"
                : "Complete";
            return;
        }

        ExecutionState = "Failed";
    }
}
