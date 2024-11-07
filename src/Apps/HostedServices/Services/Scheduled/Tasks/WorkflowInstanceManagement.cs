using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using cCoder.Security.Objects.Entities;
using cCoder.Security.Services.Processing.Interfaces;
using HostedServices.Services.Scheduled.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HostedServices.Services.Scheduled.Tasks;

internal sealed class WorkflowInstanceManagement(
    IServiceProvider serviceProvider,
    Config config,
    ILogger<WorkflowInstanceManagement> log)
        : IScheduled1MinuteOperation, IWorkflowInstanceManagement
{
    public async Task Run()
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            using ICoreDataContext core = scope.ServiceProvider.GetService<ICoreDataContext>();
            DropOldInstances(core);
            await ExecuteWaitingQueuedInstances(core);
        }
        catch (Exception ex)
        {
            log.LogError(ex, ex.Message);

            if (ex.InnerException is not null)
                log.LogError(ex.InnerException, ex.InnerException.Message);
        }
    }

    public dynamic[] GetStats()
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        using ICoreDataContext core = scope.ServiceProvider.GetService<ICoreDataContext>();

        Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<FlowInstanceData, cCoder.Core.Objects.Entities.CMS.App> problems = core.GetAll<FlowInstanceData>(false)
            .IgnoreQueryFilters()
            .Where(i => i.State == "Failed")
            .Include(i => i.FlowDefinition)
                .ThenInclude(d => d.App);

        return problems
            .Select(i => new
            {
                InstanceId = i.Id,
                FlowId = i.FlowDefinition.Id,
                Portal = i.FlowDefinition.App.Domain,
                i.Start,
                i.End,
                AppName = i.FlowDefinition.App.Name,
                FlowName = i.FlowDefinition.Name
            })
            .OrderByDescending(i => i.Start)
            .ToArray();
    }

    public async ValueTask ExecuteWaitingQueuedInstanceById(Guid id)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        using ICoreDataContext core = scope.ServiceProvider.GetService<ICoreDataContext>();

        FlowInstanceData firstInstance = core
            .GetAll<FlowInstanceData>()
            .IgnoreQueryFilters()
            .Where(f => f.State == "Queued" || f.State == "Executing")
            .Where(f => f.FlowDefinitionId == id)
            .FirstOrDefault();

        if (firstInstance is not null && firstInstance.State == "Queued")
            await ExecuteInstance(firstInstance, core);
    }

    private void DropOldInstances(ICoreDataContext core)
    {
        int dropCount = core.FlushWFInstances(DateTimeOffset.UtcNow.AddDays(-7));

        if (dropCount > 0)
            log.LogInformation($"Dropped {dropCount} Workflow instances older than 7 days.");
    }

    private async ValueTask ExecuteWaitingQueuedInstances(ICoreDataContext core)
    {
        IEnumerable<IGrouping<Guid, FlowInstanceData>> instanceGroups = core
            .GetAll<FlowInstanceData>().Where(f => f.State == "Queued" || f.State == "Executing")
            .AsEnumerable()
            .GroupBy(f => f.FlowDefinitionId);

        foreach (IGrouping<Guid, FlowInstanceData> instanceGroup in instanceGroups)
        {
            FlowInstanceData[] orderedSet = instanceGroup
                .OrderBy(i => i.Start)
                .ToArray();

            FlowInstanceData nextInstance = orderedSet.First();
            bool isQueued = nextInstance.State == "Queued";
            bool isApparentlyHung = nextInstance.State == "Executing" && nextInstance.Start < DateTimeOffset.UtcNow.AddMinutes(-15);

            if (isQueued || isApparentlyHung)
                await ExecuteInstance(nextInstance, core);
        }
    }

    private async ValueTask ExecuteInstance(FlowInstanceData instance, ICoreDataContext core)
    {
        try
        {
            instance.Start = DateTimeOffset.UtcNow;
            instance.State = "Executing";
            await core.SaveChangesAsync();

            ITokenProcessingService tokenProcessingService = serviceProvider.GetService<ITokenProcessingService>();
            Token token = await tokenProcessingService.AddTokenForUserIdAsync(instance.Caller);

            FlowDefinition definition = core.GetAll<FlowDefinition>()
                .IgnoreQueryFilters()
                .Include(f => f.App)
                .FirstOrDefault(f => f.Id == instance.FlowDefinitionId);

            await definition.Execute(config, instance.Id, token.Id);
        }
        catch
        {
            instance.State = "Failed";
            await core.SaveChangesAsync();
        }
    }
}