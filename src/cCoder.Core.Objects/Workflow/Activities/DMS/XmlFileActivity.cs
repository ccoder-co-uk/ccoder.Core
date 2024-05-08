using cCoder.Core.Objects.Dtos.Workflow;

namespace cCoder.Core.Objects.Workflow.Activities.DMS;

public class XmlFileActivity : DMSActivity
{
    [IgnoreWhenFlowComplete]
    public dynamic Result { get; set; }

    public override async Task Execute()
    {
        using System.Net.Http.HttpClient api = GetHttpClient();
        Result = Data.ParseXml<dynamic>(await GetFileContents(api));
    }
}