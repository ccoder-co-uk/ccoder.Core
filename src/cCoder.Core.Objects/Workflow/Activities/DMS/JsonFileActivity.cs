using cCoder.Core.Objects.Dtos.Workflow;

namespace cCoder.Core.Objects.Workflow.Activities.DMS;

public class JsonFileActivity : DMSActivity
{
    [IgnoreWhenFlowComplete]
    public object Result { get; set; }

    public override async Task Execute()
    {
        using System.Net.Http.HttpClient api = GetHttpClient();
        string results = await GetFileContents(api);
        Result = Data.ParseJson(results);
    }
}