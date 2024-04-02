using Core.Objects.Entities.Planning;
using Core.Objects.Extensions;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities
{
    public class QueueBackgroundJob : CoreActivity
    {
        public object JobData { get; set; }
        public string OperationName { get; set; }

        public override async Task Execute()
        {
            using var api = GetHttpClient();

            var job = new BackgroundJob
            {
                AppId = AppId,
                JobJson = JobData.ToJson(),
                OperationName = OperationName
            };

            _ = await api.AddAsync("Core/BackgroundJob", job);
        }
    }
}