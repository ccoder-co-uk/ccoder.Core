using cCoder.Workflow.Activities;
using cCoder.Workflow.Activities.Activities;
using cCoder.Workflow.Activities.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Workflow.Services;

public sealed class WorkflowScriptExecutionService(ILogger<WorkflowScriptExecutionService> logger)
{
    private static readonly string[] Imports = Activity.ScriptImports;

    public async Task<string> ExecuteAsync(string payload, bool useDetails)
    {
        ScriptRunner runner = new(LogAsync);

        if (useDetails)
        {
            ExecutionDetails details = JsonConvert.DeserializeObject<ExecutionDetails>(
                payload,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None })
                ?? throw new InvalidOperationException("Workflow script execution details could not be deserialized.");

            return await runner.Run<string>(details.Script, Imports, details.Model, LogSync);
        }

        object result = await runner.Run<object>(payload, Imports, log: LogSync);
        return JsonConvert.SerializeObject(result, WorkflowJson.GetODataJsonSettings());
    }

    private Task LogAsync(WorkflowLogLevel level, string message)
    {
        LogSync(level, message);
        return Task.CompletedTask;
    }

    private void LogSync(WorkflowLogLevel level, string message)
    {
        if (level == WorkflowLogLevel.Error || level == WorkflowLogLevel.Fatal)
            logger.LogError("{Message}", message);
        else if (level == WorkflowLogLevel.Warning)
            logger.LogWarning("{Message}", message);
        else if (level == WorkflowLogLevel.Info)
            logger.LogInformation("{Message}", message);
        else
            logger.LogDebug("{Message}", message);
    }

    public sealed class ExecutionDetails
    {
        public string Script { get; set; }

        public JObject Model { get; set; }
    }
}
