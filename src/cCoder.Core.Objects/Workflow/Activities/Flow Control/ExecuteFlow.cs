using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Objects.Workflow.Activities;

public class ExecuteFlow : CoreActivity
{
    public string ProcessName { get; set; }
    public string Name { get; set; }
    public object Data { get; set; }

    public override async Task Execute()
    {
        try
        {
            using HttpClient api = GetHttpClient();
            IEnumerable<FlowDefinition> defs = await api.GetODataCollection<FlowDefinition>($"Core/FlowDefinition?$filter=AppId eq {AppId} and Process/Name eq '{ProcessName}' and Name eq '{Name}'");
            if (defs?.Any() ?? false)
                _ = await api.PostAsync($"Core/FlowDefinition({defs.First().Id})/Execute", new StringContent(Data.ToJson())).ConfigureAwait(false);
            else
                Log(Dtos.Workflow.WorkflowLogLevel.Warning, "Flow not found!");
        }
        catch { Log(Dtos.Workflow.WorkflowLogLevel.Error, "Access Denied!"); }
    }
}