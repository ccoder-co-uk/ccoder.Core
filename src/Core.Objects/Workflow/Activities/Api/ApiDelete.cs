using Core.Objects.Dtos.Workflow;
using Core.Objects.Extensions;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities.Api
{
    public class ApiDelete<T> : ApiActivity<object>
    {
        [ApiIgnore]
        [IgnoreWhenFlowComplete]
        public T Data { get; set; }

        public override async Task Execute()
        {
            System.Net.Http.HttpClient api = GetHttpClient();
            Log(WorkflowLogLevel.Info, $"HTTP DELETE {api.BaseAddress}{Query.Replace("[Key]", Data.GetId().ToString())}");
            using (api)
            {
                _ = await api.DeleteAsync(Query);
            }
        }
    }
}