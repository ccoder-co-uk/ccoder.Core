using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Objects.Workflow.Activities;

public class QueueBackgroundJob : CoreActivity
{
    public object JobData { get; set; }
    public string OperationName { get; set; }

    public override async Task Execute()
    {
        using HttpClient api = GetHttpClient();

        BackgroundJob job = new()
        {
            AppId = AppId,
            JobJson = JobData.ToJson(),
            OperationName = OperationName
        };

        _ = await api.AddAsync("Core/BackgroundJob", job);
    }
}