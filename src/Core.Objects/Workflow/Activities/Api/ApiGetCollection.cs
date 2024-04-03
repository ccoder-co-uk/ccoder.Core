using Core.Objects.Dtos.Workflow;
using Core.Objects.Extensions;

namespace Core.Objects.Workflow.Activities.Api;

public class ApiGetCollection<T> : ApiActivity<IEnumerable<T>>
{
    public override async Task Execute()
    {
        using var api = GetHttpClient();
        Log(WorkflowLogLevel.Info, $"HTTP GET {api.BaseAddress}{Query}");
        Result = await api.GetODataCollection<T>(Query);
    }
}