// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models.Workflow;
using cCoder.Data.Models.Planning;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class WorkflowEventIntegrationTests
{
    [Fact]
    public async Task ScheduledTaskExecution_QueuesAndCompletesWorkflowInstance()
    {
        Guid flowId = Guid.Empty;
        int taskId = 0;

        try
        {
            flowId = await CreateFlowDefinitionAsync(appId: BaselineAppId,name: Unique(prefix: "Scheduled Flow"));
            taskId = await CreateScheduledTaskAsync(flowId: flowId,name: Unique(prefix: "Scheduled Task"));

            await PostAsync(relativeUrl: $"/Api/Workflow/ScheduledTask({taskId})/Execute?incrementNextExecution=true");

            await WaitUntilAsync(predicate: async () => await HasAnyFlowInstanceAsync(flowId: flowId));

            await WaitUntilAsync(
predicate:                 async () => await HasFlowInstanceStateAsync(flowId: flowId,state: "Complete"),                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId: flowId);

            instance.Should()
                .NotBeNull();

            instance.Caller.Should()
                .Be(expected: AdminUserId);

            instance.State.Should()
                .Be(expected: "Complete");

            instance.ContextString.Should()
                .Contain(expected: "Execution complete.");

            instance.ContextString.Should()
                .NotContain(unexpected: "Execution failed.");

            FlowInstanceData[] instances = await GetFlowInstancesAsync(flowId: flowId);

            instances.Should()
                .HaveCount(expected: 1);

            instances.Should()
                .OnlyContain(predicate: found => found.State == "Complete");
        }
        finally
        {
            await DeleteFlowArtifactsAsync(flowId: flowId,taskId: taskId);
        }
    }

    [Fact]
    public async Task ScheduledTaskRunner_ExecutesDueScheduledTask()
    {
        Guid flowId = Guid.Empty;
        int taskId = 0;

        try
        {
            flowId = await CreateFlowDefinitionAsync(appId: BaselineAppId,name: Unique(prefix: "Hosted Scheduled Flow"));

            taskId = await CreateScheduledTaskAsync(
flowId:                 flowId,name:                 Unique(prefix: "Hosted Scheduled Task"),                nextExecution: DateTimeOffset.UtcNow.AddMinutes(minutes: -5));

            await fixture.RestartHostedServicesAsync();

            await WaitUntilAsync(
predicate:                 async () => await HasAnyFlowInstanceAsync(flowId: flowId),                attempts: 60,                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            await WaitUntilAsync(
predicate:                 async () => await HasFlowInstanceStateAsync(flowId: flowId,state: "Complete"),                attempts: 60,                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId: flowId);

            instance.Should()
                .NotBeNull();

            instance.Caller.Should()
                .Be(expected: AdminUserId);

            instance.State.Should()
                .Be(expected: "Complete");

            FlowInstanceData[] instances = await GetFlowInstancesAsync(flowId: flowId);

            instances.Should()
                .HaveCount(expected: 1);

            instances.Should()
                .OnlyContain(predicate: found => found.State == "Complete");

            await using CoreDataContext core = CreateCoreContext();

            ScheduledTask task = await core.Set<ScheduledTask>()
                .IgnoreQueryFilters()
                .FirstAsync(predicate: found => found.Id == taskId);

            task.LastExecuted.Should()
                .NotBeNull();

            task.LastExecuted.Should()
                .BeAfter(expected: DateTimeOffset.UtcNow.AddMinutes(minutes: -3));

            task.NextExecution.Should()
                .NotBeNull();

            task.NextExecution.Should()
                .BeAfter(expected: DateTimeOffset.UtcNow.AddSeconds(seconds: -5));
        }
        finally
        {
            await DeleteFlowArtifactsAsync(flowId: flowId,taskId: taskId);
        }
    }

    [Fact]
    public async Task ScheduledTaskRunner_ExecutesTaskThatBecomesDueAfterStartupWithoutExceptions()
    {
        Guid flowId = Guid.Empty;
        int taskId = 0;

        try
        {
            flowId = await CreateFlowDefinitionAsync(appId: BaselineAppId,name: Unique(prefix: "Delayed Hosted Scheduled Flow"));

            taskId = await CreateScheduledTaskAsync(
flowId:                 flowId,name:                 Unique(prefix: "Delayed Hosted Scheduled Task"),                nextExecution: DateTimeOffset.UtcNow.AddHours(hours: 1));

            await fixture.RestartHostedServicesAsync();

            await WaitUntilAsync(
predicate:                 () => Task.FromResult(result: HostedServicesOutputContains(value: "No scheduled tasks are due to run.")),                attempts: 40,                delayMilliseconds: 250,                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            await UpdateScheduledTaskNextExecutionAsync(taskId: taskId,nextExecution: DateTimeOffset.UtcNow.AddMinutes(minutes: -5));

            await WaitUntilAsync(
predicate:                 async () => await HasFlowInstanceStateAsync(flowId: flowId,state: "Complete"),                attempts: 180,                delayMilliseconds: 500,                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            fixture.HostedServicesOutput.Should()
                .NotContain(unexpected: "Exception thrown whilst raising scheduled_task_execute event");

            fixture.HostedServicesOutput.Should()
                .NotContain(unexpected: "Object reference not set to an instance of an object");

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId: flowId);

            instance.Should()
                .NotBeNull();

            instance.Caller.Should()
                .Be(expected: AdminUserId);

            instance.State.Should()
                .Be(expected: "Complete");

            FlowInstanceData[] instances = await GetFlowInstancesAsync(flowId: flowId);

            instances.Should()
                .HaveCount(expected: 1);

            instances.Should()
                .OnlyContain(predicate: found => found.State == "Complete");

            await using CoreDataContext core = CreateCoreContext();

            ScheduledTask task = await core.Set<ScheduledTask>()
                .IgnoreQueryFilters()
                .FirstAsync(predicate: found => found.Id == taskId);

            task.LastExecuted.Should()
                .NotBeNull();

            task.NextExecution.Should()
                .NotBeNull();

            task.NextExecution.Should()
                .BeAfter(expected: DateTimeOffset.UtcNow.AddMinutes(minutes: -1));
        }
        finally
        {
            await DeleteFlowArtifactsAsync(flowId: flowId,taskId: taskId);
        }
    }

    [Fact]
    public async Task ScheduledTaskRunner_QueuesTaskForExecuteOnlyUserWithoutReadPrivilege()
    {
        Guid flowId = Guid.Empty;
        int taskId = 0;
        string executeOnlyUserId = null;
        Guid executeOnlyRoleId = Guid.Empty;

        try
        {
            (executeOnlyUserId, executeOnlyRoleId) = await CreateExecuteOnlyUserAsync(appId: BaselineAppId);

            flowId = await CreateFlowDefinitionAsync(appId: BaselineAppId,name: Unique(prefix: "Execute Only Scheduled Flow"));

            taskId = await CreateScheduledTaskAsync(
flowId:                 flowId,name:                 Unique(prefix: "Execute Only Scheduled Task"),                nextExecution: DateTimeOffset.UtcNow.AddMinutes(minutes: -5),                executeAs: executeOnlyUserId);

            await fixture.RestartHostedServicesAsync();

            await WaitUntilAsync(
predicate:                 async () => await HasAnyFlowInstanceAsync(flowId: flowId),                attempts: 60,                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            fixture.HostedServicesOutput.Should()
                .NotContain(unexpected: "Exception thrown whilst raising scheduled_task_execute event");

            fixture.HostedServicesOutput.Should()
                .NotContain(unexpected: "Access Denied!");

            await WaitUntilAsync(
predicate:                 async () =>
                {
                    FlowInstanceData latestInstance = await GetLatestInstanceAsync(flowId: flowId);

                    return latestInstance.State != "Queued";
                },                attempts: 180,                delayMilliseconds: 500,                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId: flowId);

            instance.Should()
                .NotBeNull();

            instance.Caller.Should()
                .Be(expected: executeOnlyUserId);

            instance.State.Should()
                .NotBe(unexpected: "Queued");

            FlowInstanceData[] instances = await GetFlowInstancesAsync(flowId: flowId);

            instances.Should()
                .HaveCount(expected: 1);
        }
        finally
        {
            await DeleteFlowArtifactsAsync(flowId: flowId,taskId: taskId);
            await DeleteExecuteOnlyUserAsync(userId: executeOnlyUserId,roleId: executeOnlyRoleId);
        }
    }
}