using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security;

namespace cCoder.Core.Services.Workflow;

public class FlowInstanceDataService(ICoreDataContext db, Config config) 
    : CoreService<FlowInstanceData>(db)
{
    public override async Task<FlowInstanceData> UpdateAsync(FlowInstanceData entity)
    {
        FlowInstanceData dbVersion = Db.GetAll<FlowInstanceData>()
            .Include(fd => fd.FlowDefinition)
            .FirstOrDefault(f => f.Id == entity.Id);

        dbVersion.UpdateFrom(entity);

        FlowInstanceData result = User.Can(dbVersion.FlowDefinition.AppId, "flowinstancedata_update")
            ? await Db.UpdateAsync(dbVersion)
            : throw new SecurityException("Access Denied!");

        ExecuteNextQueuedInstance(result.FlowDefinitionId);

        return result;
    }

    private void ExecuteNextQueuedInstance(Guid flowDefinitionId)
    {
        using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
        {
            BaseAddress = new Uri(config.Services["Scheduler"])
        };

        api.PostAsync("Workflow/ExecuteNextFlowInstanceInQueue?flowId=" + flowDefinitionId, null)
            .Forget();
    }
}