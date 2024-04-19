using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security;

namespace cCoder.Core.Services.Workflow;

public class FlowDefinitionService(Config config, ICoreDataContext db, ILogger<FlowDefinitionService> log) 
    : CoreService<FlowDefinition>(db), IFlowDefinitionService
{
    public async Task<Guid> Queue(Guid id, string args)
    {
        var flow = Db.GetAll<FlowDefinition>(false)
            .Include(f => f.App)
            .FirstOrDefault(f => f.Id == id);

        var result = flow != null
            ? await flow.QueueNewInstance(Db, Db.User, args)
            : throw new SecurityException("Access Denied!");

        ExecuteNextQueuedInstance(id);

        return result;
    }

    public override async Task DeleteAsync(object id)
    {
        var flow = Db.GetAll<FlowDefinition>(true)
            .Include(fd => fd.Instances)
            .FirstOrDefault(fd => fd.Id == (Guid)id);

        if (flow != null)
        {
            await Db.DeleteAllAsync(flow.Instances);
            _ = await Db.DeleteAsync(flow);
        }
        else
            throw new SecurityException("Access Denied!");
    }

    public override Task<IEnumerable<Result<FlowDefinition>>> AddAllAsync(IEnumerable<FlowDefinition> items)
    {
        log.LogDebug($"Adding:\n{items.Select(i => new { i.Id, i.Name }).ToJsonForOdata()}");
        return base.AddAllAsync(items);
    }

    public override Task<IEnumerable<Result<FlowDefinition>>> UpdateAllAsync(IEnumerable<FlowDefinition> items)
    {
        log.LogDebug($"Updating:\n{items.Select(i => new { i.Id, i.Name }).ToJsonForOdata()}");
        return base.UpdateAllAsync(items);
    }

    void ExecuteNextQueuedInstance(Guid flowDefinitionId)
    {
        using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
        {
            BaseAddress = new Uri(config.Services["Scheduler"])
        };

        api.PostAsync("Workflow/ExecuteNextFlowInstanceInQueue?flowId=" + flowDefinitionId, null)
            .Forget();
    }
}