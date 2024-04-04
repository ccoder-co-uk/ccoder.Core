using cCoder.Core.Objects.Dtos.Workflow;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Workflow.Activities.DMS
{
    public class TextFileContentActivity : DMSActivity
    {
        [IgnoreWhenFlowComplete]
        public string Result { get; set; }

        public override async Task Execute()
        {
            using System.Net.Http.HttpClient api = GetHttpClient();
            Result = await GetFileContents(api);
        }
    }
}