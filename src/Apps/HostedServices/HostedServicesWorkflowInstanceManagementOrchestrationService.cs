// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Text;
using System.Text.Json;
using cCoder.Data;
using cCoder.Data.Models.Workflow;
using cCoder.Security.Exposures;
using cCoder.Security.Models.Entities;
using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Brokers;
using cCoder.Workflow.Exposures;
using cCoder.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HostedServices;

internal sealed class HostedServicesWorkflowInstanceManagementOrchestrationService(
    IWorkflowInstanceManagementBroker workflowInstanceManagementBroker,
    ICoreContextFactory coreContextFactory,
    IServiceProvider serviceProvider,
    CoreConfiguration configuration,
    ILogger<HostedServicesWorkflowInstanceManagementOrchestrationService> log)
    : IWorkflowInstanceManager
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await RunInstanceMaintenanceAsync(cancellationToken: cancellationToken);
            await RunQueueInstanceBackgroundServiceDependencyAsync(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            log.LogError(
                exception: exception,
                message: "Workflow processing failed: {ErrorMessage}",
                args: exception.Message);

            if (exception.InnerException is not null)
            {
                log.LogError(
                    exception: exception.InnerException,
                    message: "Inner workflow processing failure: {ErrorMessage}",
                    args: exception.InnerException.Message);
            }
        }
    }

    public object[] GetStats() =>
        workflowInstanceManagementBroker.GetFailedExecutionStats();

    public async ValueTask ExecuteWaitingQueuedInstanceByIdAsync(Guid id)
    {
        await ExecuteInstanceAsync(instanceId: id);
    }

    public async Task RunInstanceMaintenanceContinuouslyAsync(CancellationToken cancellationToken = default)
    {
        await RunInstanceMaintenanceAsync(cancellationToken: cancellationToken);

        using PeriodicTimer timer = new(TimeSpan.FromMinutes(minutes: 1));

        while (!cancellationToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(cancellationToken: cancellationToken))
        {
            await RunInstanceMaintenanceAsync(cancellationToken: cancellationToken);
        }
    }

    public async Task RunInstanceMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DropOldInstancesAsync(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            log.LogError(
                exception: exception,
                message: "Workflow maintenance failed: {ErrorMessage}",
                args: exception.Message);

            if (exception.InnerException is not null)
            {
                log.LogError(
                    exception: exception.InnerException,
                    message: "Inner workflow maintenance failure: {ErrorMessage}",
                    args: exception.InnerException.Message);
            }
        }
    }

    public async Task RunQueueInstanceBackgroundServiceDependencyContinuouslyAsync(
        CancellationToken cancellationToken = default)
    {
        await RunQueueInstanceBackgroundServiceDependencyAsync(cancellationToken: cancellationToken);

        using PeriodicTimer timer = new(GetQueuePollingInterval());

        while (!cancellationToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(cancellationToken: cancellationToken))
        {
            await RunQueueInstanceBackgroundServiceDependencyAsync(cancellationToken: cancellationToken);
        }
    }

    public async Task RunQueueInstanceBackgroundServiceDependencyAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await ExecuteWaitingQueuedInstancesAsync(cancellationToken: cancellationToken);
            await RequeueHungExecutingInstancesAsync(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            log.LogError(
                exception: exception,
                message: "Workflow queue processing failed: {ErrorMessage}",
                args: exception.Message);

            if (exception.InnerException is not null)
            {
                log.LogError(
                    exception: exception.InnerException,
                    message: "Inner workflow queue processing failure: {ErrorMessage}",
                    args: exception.InnerException.Message);
            }
        }
    }

    private TimeSpan GetQueuePollingInterval()
    {
        int pollingIntervalMilliseconds =
            configuration.Workflow.QueueInstanceManagement
                .PollingIntervalMilliseconds;

        return TimeSpan.FromMilliseconds(
            value: pollingIntervalMilliseconds);
    }

    private async ValueTask DropOldInstancesAsync(CancellationToken cancellationToken)
    {
        int dropCount = await workflowInstanceManagementBroker
            .FlushOldInstancesAsync(cutoff: DateTimeOffset.UtcNow.AddDays(days: -7),cancellationToken: cancellationToken);

        if (dropCount > 0)
        {
            if (log.IsEnabled(logLevel: LogLevel.Information))
            {
                log.LogInformation(
                    message: "Dropped {Count} Workflow instances older than 7 days.",
                    args: dropCount);
            }
        }
    }

    private async ValueTask RequeueHungExecutingInstancesAsync(CancellationToken cancellationToken)
    {
        int requeueCount = await workflowInstanceManagementBroker
            .RequeueHungExecutingInstancesAsync(cutoff: DateTimeOffset.UtcNow.AddMinutes(minutes: -30),cancellationToken: cancellationToken);

        if (requeueCount > 0)
        {
            log.LogWarning(
message:                 "Requeued {Count} Workflow instances that were still executing after 30 minutes.",args:                 requeueCount);
        }
    }

    private async ValueTask ExecuteWaitingQueuedInstancesAsync(CancellationToken cancellationToken)
    {
        List<Task> executions = [];

        foreach (Guid instanceId in workflowInstanceManagementBroker.GetQueuedInstances()
            .Select(selector: instance => instance.Id)
            .Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();
            executions.Add(item: ExecuteInstanceAsync(instanceId: instanceId,cancellationToken: cancellationToken));
        }

        await Task.WhenAll(tasks: executions);
    }

    private async Task ExecuteInstanceAsync(
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        int claimedCount = await workflowInstanceManagementBroker
            .UpdateQueuedInstanceClaimAsync(
                flowInstanceDataId: instanceId,
                cancellationToken: cancellationToken);

        if (claimedCount == 0)
        {
            return;
        }

        FlowInstanceData dbInstance = await workflowInstanceManagementBroker
            .SelectClaimedInstanceAsync(
                flowInstanceDataId: instanceId,
                cancellationToken: cancellationToken);

        if (dbInstance is null)
        {
            return;
        }

        try
        {
            ITokenManager tokenManager = serviceProvider.GetRequiredService<ITokenManager>();
            Token token = await tokenManager.IssueTokenAsync(userId: dbInstance.Caller,tokenUse: TokenUse.WorkflowExecution);

            WorkflowRequest request = new()
            {
                Api = $"https://{dbInstance.FlowDefinition.App.Domain}:{configuration.Workflow.SslPort}/Api/",
                FlowId = dbInstance.FlowDefinition.Id,
                AuthToken = token.Id,
                InstanceId = dbInstance.Id
            };

            HttpResponseMessage result = await SendToWorkflowAsync(request: request,cancellationToken: cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                string error = await result.Content.ReadAsStringAsync(cancellationToken: cancellationToken);

                log.LogError(
                    "Flow instance {InstanceId} execution failed.{NewLine}{ErrorDetails}",
                    dbInstance.Id,
                    Environment.NewLine,
                    error);

                await MarkFailedAsync(
instanceId:                     dbInstance.Id,context:                     $"Workflow host returned {(int)result.StatusCode} ({result.StatusCode}).{Environment.NewLine}{error}",cancellationToken:                     cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            log.LogError(exception: exception,message: "Flow instance {InstanceId} execution threw an exception.",args: dbInstance.Id);

            await MarkFailedAsync(
instanceId:                 dbInstance.Id,context:                 $"Workflow execution failed.{Environment.NewLine}{exception.Message}",cancellationToken:                 cancellationToken);

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
            BaseAddress = new Uri(configuration.Workflow.ServiceUrl)
        };

        return await api.PostAsync(
requestUri:             "Execute",content:             new StringContent(
                JsonSerializer.Serialize(value: request),
                Encoding.UTF8,
                "application/json"),cancellationToken:             cancellationToken);
    }

    private async Task MarkFailedAsync(
        Guid instanceId,
        string context,
        CancellationToken cancellationToken)
    {
        using CoreDataContext core = coreContextFactory.CreateCoreContext();

        FlowInstanceData instance = await core.FlowInstances
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(predicate: found => found.Id == instanceId,cancellationToken: cancellationToken);

        if (instance is null)
        {
            return;
        }

        instance.State = "Failed";
        instance.End = DateTimeOffset.UtcNow;
        instance.ContextString = context;

        _ = await core.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}