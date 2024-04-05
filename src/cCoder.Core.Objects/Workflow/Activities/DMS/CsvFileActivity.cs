using cCoder.Core.Objects.Dtos.Workflow;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Workflow.Activities.DMS
{
    public class CsvFileActivity : DMSActivity
    {
        public CSVParseConfig CSVParseConfig { get; set; }

        [IgnoreWhenFlowComplete]
        public dynamic Result { get; set; }

        public override async Task Execute()
        {
            using System.Net.Http.HttpClient api = GetHttpClient();
            Result = Data.ParseCSV<dynamic>(await GetFileContents(api), CSVParseConfig);
        }
    }
}