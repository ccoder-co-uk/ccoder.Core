using System.Net;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Workflow;
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
    static HttpClientHandler handler = new HttpClientHandler() 
    { 
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate 
    };

    public async Task Run()
    {
        try
        {
            DropOldInstances();
            await ExecuteWaitingQueuedInstances();
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
            await ExecuteInstance(firstInstance);
    }

    private void DropOldInstances()
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        using ICoreDataContext core = scope.ServiceProvider.GetService<ICoreDataContext>();

        int dropCount = core.FlushWFInstances(DateTimeOffset.UtcNow.AddDays(-7));

        if (dropCount > 0)
            log.LogInformation($"Dropped {dropCount} Workflow instances older than 7 days.");
    }

    private async ValueTask ExecuteWaitingQueuedInstances()
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        using ICoreDataContext core = scope.ServiceProvider.GetService<ICoreDataContext>();

        IGrouping<Guid, FlowInstanceData>[] instanceGroups = core
            .GetAll<FlowInstanceData>().Where(f => f.State == "Queued" || f.State == "Executing")
            .Include(i => i.FlowDefinition)
                .ThenInclude(d => d.App)
            .GroupBy(f => f.FlowDefinitionId)
            .ToArray();

        bool isQueued;
        bool isApparentlyHung;

        List<Task> executions = [];

        foreach (IGrouping<Guid, FlowInstanceData> instanceGroup in instanceGroups)
        {
            FlowInstanceData[] orderedSet = instanceGroup
                .OrderBy(i => i.Start)
                .ToArray();

            FlowInstanceData nextInstance = orderedSet.First();
            isQueued = nextInstance.State == "Queued";
            isApparentlyHung = nextInstance.State == "Executing" && nextInstance.Start < DateTimeOffset.UtcNow.AddMinutes(-15);

            if (isQueued || isApparentlyHung)
                executions.Add(ExecuteInstance(nextInstance));
        }

        await Task.WhenAll(executions);
    }

    private async Task ExecuteInstance(FlowInstanceData instance)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        using ICoreDataContext core = scope.ServiceProvider.GetService<ICoreDataContext>();

        var dbInstance = await core
            .GetAll<FlowInstanceData>()
            .IgnoreQueryFilters()
            .Where(i => i.Id == instance.Id)
            .FirstOrDefaultAsync();

        dbInstance.Start = DateTimeOffset.UtcNow;
        dbInstance.State = "Executing";
        await core.SaveChangesAsync();

        ITokenProcessingService tokenProcessingService = serviceProvider.GetService<ITokenProcessingService>();
        Token token = await tokenProcessingService.AddTokenForUserIdAsync(instance.Caller);

        var definition = instance.FlowDefinition;

        var request = new WorkflowRequest
        {
            Api = $"https://{instance.FlowDefinition.App.Domain}:{config.Settings["sslPort"] ?? "443"}/Api/",
            FlowId = instance.FlowDefinition.Id,
            AuthToken = token.Id,
            InstanceId = instance.Id
        };

        HttpResponseMessage result = await SendToWorkflow(request);

        if (!result.IsSuccessStatusCode)
            log.LogError("Flow instance {InstanceId} execution failed.\n{ErrorDetails}", dbInstance.Id, result.Content.ReadAsStringAsync());
    }

    private async ValueTask<HttpResponseMessage> SendToWorkflow(WorkflowRequest request)
    {
        using HttpClient api = new(handler)
        {
            BaseAddress = new Uri(config.Services["Workflow"])
        };

        return await api.PostAsync(
            "Execute", 
            new StringContent(request.ToJson(), System.Text.Encoding.UTF8, "application/json"));
    }
}