using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Objects.Workflow.Activities.Api
{
    public class ApiDelete<T> : ApiActivity<object>
    {
        [ApiIgnore]
        [IgnoreWhenFlowComplete]
        public T Data { get; set; }

        public override async Task Execute()
        {
            using var api = GetHttpClient();

            Log(WorkflowLogLevel.Info, $"HTTP DELETE {api.BaseAddress}{Query.Replace("[Key]", Data.GetId().ToString())}");

            await api.DeleteAsync(Query);
        }
    }
}