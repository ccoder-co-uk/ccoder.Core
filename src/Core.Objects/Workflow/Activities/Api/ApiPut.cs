using Core.Objects.Dtos.Workflow;
using Core.Objects.Extensions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities.Api
{
    public class ApiPut<T, TResult> : ApiActivity<TResult>
    {
        [ApiIgnore]
        [IgnoreWhenFlowComplete]
        public T Data { get; set; }

        public override async Task Execute()
        {
            HttpClient api = GetHttpClient();
            Log(WorkflowLogLevel.Info, $"HTTP PUT {api.BaseAddress}{Query.Replace("[Key]", Data.GetId().ToString())}");
            using (api)
            {
                HttpResponseMessage response = await api.PostAsync(Query.Replace("[Key]", Data.GetId().ToString()), new StringContent(Data.ToJsonForOdata(), Encoding.UTF8, "application/json"));
                _ = response.EnsureSuccessStatusCode();
                Result = await response.Content.ReadAsAsync<TResult>();
            }
        }
    }
}