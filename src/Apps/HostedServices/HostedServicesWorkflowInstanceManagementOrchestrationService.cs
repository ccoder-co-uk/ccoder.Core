using System.Net;
using System.Text;
using System.Text.Json;
using cCoder.Data;
using cCoder.Data.Models.Workflow;
using cCoder.Security.Exposures;
using cCoder.Security.Objects.Entities;
using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Brokers;
using cCoder.Workflow.Services.Orchestrations;
using Microsoft.EntityFrameworkCore;

namespace HostedServices;

internal sealed class HostedServicesWorkflowInstanceManagementOrchestrationService(
    IWorkflowInstanceManagementBroker workflowInstanceManagementBroker,
    ICoreContextFactory coreContextFactory,
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<HostedServicesWorkflowInstanceManagementOrchestrationService> log)
    : IWorkflowInstanceManagementOrchestrationService
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DropOldInstancesAsync(cancellationToken);
            await RequeueHungExecutingInstancesAsync(cancellationToken);
            await ExecuteWaitingQueuedInstancesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            log.LogError(exception, exception.Message);

            if (exception.InnerException is not null)
                log.LogError(exception.InnerException, exception.InnerException.Message);
        }
    }

    public object[] GetStats() =>
        workflowInstanceManagementBroker.GetFailedExecutionStats();

    public async ValueTask ExecuteWaitingQueuedInstanceByIdAsync(Guid id)
    {
        await ExecuteInstanceAsync(id);
    }

    private async ValueTask DropOldInstancesAsync(CancellationToken cancellationToken)
    {
        int dropCount = await workflowInstanceManagementBroker
            .FlushOldInstancesAsync(DateTimeOffset.UtcNow.AddDays(-7), cancellationToken);

        if (dropCount > 0)
            log.LogInformation("Dropped {Count} Workflow instances older than 7 days.", dropCount);
    }

    private async ValueTask RequeueHungExecutingInstancesAsync(CancellationToken cancellationToken)
    {
        int requeueCount = await workflowInstanceManagementBroker
            .RequeueHungExecutingInstancesAsync(DateTimeOffset.UtcNow.AddMinutes(-30), cancellationToken);

        if (requeueCount > 0)
        {
            log.LogWarning(
                "Requeued {Count} Workflow instances that were still executing after 30 minutes.",
                requeueCount);
        }
    }

    private async ValueTask ExecuteWaitingQueuedInstancesAsync(CancellationToken cancellationToken)
    {
        List<Task> executions = [];

        foreach (Guid instanceId in workflowInstanceManagementBroker.GetQueuedInstances()
            .Select(instance => instance.Id)
            .Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();
            executions.Add(ExecuteInstanceAsync(instanceId, cancellationToken));
        }

        await Task.WhenAll(executions);
    }

    private async Task ExecuteInstanceAsync(
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        FlowInstanceData dbInstance = await workflowInstanceManagementBroker
            .ClaimQueuedInstanceAsync(instanceId, cancellationToken);

        if (dbInstance is null)
            return;

        try
        {
            IAccountManager accountManager = serviceProvider.GetRequiredService<IAccountManager>();
            Token token = await accountManager.IssueTokenAsync(dbInstance.Caller);

            WorkflowRequest request = new()
            {
                Api = $"https://{dbInstance.FlowDefinition.App.Domain}:{configuration["Settings:sslPort"] ?? "443"}/Api/",
                FlowId = dbInstance.FlowDefinition.Id,
                AuthToken = token.Id,
                InstanceId = dbInstance.Id
            };

            HttpResponseMessage result = await SendToWorkflowAsync(request, cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                string error = await result.Content.ReadAsStringAsync(cancellationToken);

                log.LogError(
                    "Flow instance {InstanceId} execution failed.{NewLine}{ErrorDetails}",
                    dbInstance.Id,
                    Environment.NewLine,
                    error);

                await MarkFailedAsync(
                    dbInstance.Id,
                    $"Workflow host returned {(int)result.StatusCode} ({result.StatusCode}).{Environment.NewLine}{error}",
                    cancellationToken);
            }
        }
        catch (Exception exception)
        {
            log.LogError(exception, "Flow instance {InstanceId} execution threw an exception.", dbInstance.Id);
            await MarkFailedAsync(
                dbInstance.Id,
                $"Workflow execution failed.{Environment.NewLine}{exception.Message}",
                cancellationToken);
            throw;
        }
    }

    private async ValueTask<HttpResponseMessage> SendToWorkflowAsync(
        WorkflowRequest request,
        CancellationToken cancellationToken)
    {
        using HttpClient api = new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        {
            BaseAddress = new Uri(configuration["Services:Workflow"])
        };

        return await api.PostAsync(
            "Execute",
            new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"),
            cancellationToken);
    }

    private async Task MarkFailedAsync(
        Guid instanceId,
        string context,
        CancellationToken cancellationToken)
    {
        using CoreDataContext core = coreContextFactory.CreateCoreContext();

        FlowInstanceData instance = await core.FlowInstances
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(found => found.Id == instanceId, cancellationToken);

        if (instance is null)
            return;

        instance.State = "Failed";
        instance.End = DateTimeOffset.UtcNow;
        instance.ContextString = context;

        _ = await core.SaveChangesAsync(cancellationToken);
    }
}
