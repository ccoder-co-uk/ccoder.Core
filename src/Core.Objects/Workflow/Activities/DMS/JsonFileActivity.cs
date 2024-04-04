using cCoder.Core.Objects.Dtos.Workflow;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Workflow.Activities.DMS
{
    public class JsonFileActivity : DMSActivity
    {
        [IgnoreWhenFlowComplete]
        public object Result { get; set; }

        public override async Task Execute()
        {
            using System.Net.Http.HttpClient api = GetHttpClient();
            var results = await GetFileContents(api);
            Result = Data.ParseJson(results);
        }
    }
}