using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Objects.Workflow.Activities.Api;

public class ApiGetCollection<T> : ApiActivity<IEnumerable<T>>
{
    public override async Task Execute()
    {
        using HttpClient api = GetHttpClient();
        Log(WorkflowLogLevel.Info, $"HTTP GET {api.BaseAddress}{Query}");
        Result = await api.GetODataCollection<T>(Query);
    }
}