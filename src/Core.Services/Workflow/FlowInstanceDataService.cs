using Core.Objects;
using Core.Objects.Entities.Workflow;
using Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security;

namespace Core.Services.Workflow;

public class FlowInstanceDataService(
    ICoreDataContext db,
    Config config,
    ILogger<FlowInstanceDataService> log) : CoreService<FlowInstanceData>(db)
{
    public override async Task<FlowInstanceData> UpdateAsync(FlowInstanceData entity)
    {
        FlowInstanceData dbVersion = Db.GetAll<FlowInstanceData>()
            .Include(fd => fd.FlowDefinition)
            .FirstOrDefault(f => f.Id == entity.Id);

        bool executeNextInQueue = dbVersion.State == "Executing" && entity.State != "Executing";

        dbVersion.UpdateFrom(entity);

        var result = User.Can(dbVersion.FlowDefinition.AppId, "flowinstancedata_update")
            ? await Db.UpdateAsync(dbVersion)
            : throw new SecurityException("Access Denied!");

        if (executeNextInQueue)
            await ExecuteNextQueuedInstance(result.FlowDefinitionId);

        return result;
    }

    async Task ExecuteNextQueuedInstance(Guid flowDefinitionId)
    {
        try
        {
            var scheduler = config.Services["Scheduler"];

            using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            {
                BaseAddress = new Uri(scheduler)
            };

            api.Timeout = TimeSpan.FromMinutes(11);

            var response = await api.PostAsync("Workflow/ExecuteNextFlowInstanceInQueue?flowId=" + flowDefinitionId, null);
            _ = response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            log.LogWarning($"Failed to execute next flow instance for flow `{flowDefinitionId}`:\n{ex.Message}");
        }
    }
}