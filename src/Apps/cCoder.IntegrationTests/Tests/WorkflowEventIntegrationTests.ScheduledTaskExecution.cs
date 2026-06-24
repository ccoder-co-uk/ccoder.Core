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
            flowId = await CreateFlowDefinitionAsync(BaselineAppId, Unique("Scheduled Flow"));
            taskId = await CreateScheduledTaskAsync(flowId, Unique("Scheduled Task"));

            await PostAsync($"/Api/Core/ScheduledTask({taskId})/Execute?incrementNextExecution=true");

            await WaitUntilAsync(async () => await HasAnyFlowInstanceAsync(flowId));

            await WaitUntilAsync(
                async () => await HasFlowInstanceStateAsync(flowId, "Complete"),
                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId));

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId);
            instance.Should().NotBeNull();
            instance.Caller.Should().Be(AdminUserId);
            instance.State.Should().Be("Complete");
            instance.ContextString.Should().Contain("Execution complete.");
            instance.ContextString.Should().NotContain("Execution failed.");

            FlowInstanceData[] instances = await GetFlowInstancesAsync(flowId);
            instances.Should().HaveCount(1);
            instances.Should().OnlyContain(found => found.State == "Complete");
        }
        finally
        {
            await DeleteFlowArtifactsAsync(flowId, taskId);
        }
    }

    [Fact]
    public async Task ScheduledTaskRunner_ExecutesDueScheduledTask()
    {
        Guid flowId = Guid.Empty;
        int taskId = 0;

        try
        {
            flowId = await CreateFlowDefinitionAsync(BaselineAppId, Unique("Hosted Scheduled Flow"));
            taskId = await CreateScheduledTaskAsync(
                flowId,
                Unique("Hosted Scheduled Task"),
                nextExecution: DateTimeOffset.UtcNow.AddMinutes(-5));

            await fixture.RestartHostedServicesAsync();

            await WaitUntilAsync(
                async () => await HasAnyFlowInstanceAsync(flowId),
                attempts: 60,
                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId));

            await WaitUntilAsync(
                async () => await HasFlowInstanceStateAsync(flowId, "Complete"),
                attempts: 60,
                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId));

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId);
            instance.Should().NotBeNull();
            instance.Caller.Should().Be(AdminUserId);
            instance.State.Should().Be("Complete");
            FlowInstanceData[] instances = await GetFlowInstancesAsync(flowId);
            instances.Should().HaveCount(1);
            instances.Should().OnlyContain(found => found.State == "Complete");

            await using CoreDataContext core = CreateCoreContext();
            ScheduledTask task = await core.Set<ScheduledTask>().IgnoreQueryFilters()
                .FirstAsync(found => found.Id == taskId);

            task.LastExecuted.Should().NotBeNull();
            task.LastExecuted.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-3));
            task.NextExecution.Should().NotBeNull();
            task.NextExecution.Should().BeAfter(DateTimeOffset.UtcNow.AddSeconds(-5));
        }
        finally
        {
            await DeleteFlowArtifactsAsync(flowId, taskId);
        }
    }

    [Fact]
    public async Task ScheduledTaskRunner_ExecutesTaskThatBecomesDueAfterStartupWithoutExceptions()
    {
        Guid flowId = Guid.Empty;
        int taskId = 0;

        try
        {
            flowId = await CreateFlowDefinitionAsync(BaselineAppId, Unique("Delayed Hosted Scheduled Flow"));
            taskId = await CreateScheduledTaskAsync(
                flowId,
                Unique("Delayed Hosted Scheduled Task"),
                nextExecution: DateTimeOffset.UtcNow.AddHours(1));

            await fixture.RestartHostedServicesAsync();

            await WaitUntilAsync(
                () => Task.FromResult(HostedServicesOutputContains("No scheduled tasks are due to run.")),
                attempts: 40,
                delayMilliseconds: 250,
                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId));

            await UpdateScheduledTaskNextExecutionAsync(taskId, DateTimeOffset.UtcNow.AddMinutes(-5));

            await WaitUntilAsync(
                async () => await HasFlowInstanceStateAsync(flowId, "Complete"),
                attempts: 180,
                delayMilliseconds: 500,
                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId));

            fixture.HostedServicesOutput.Should().NotContain("Exception thrown whilst raising scheduled_task_execute event");
            fixture.HostedServicesOutput.Should().NotContain("Object reference not set to an instance of an object");

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId);
            instance.Should().NotBeNull();
            instance.Caller.Should().Be(AdminUserId);
            instance.State.Should().Be("Complete");
            FlowInstanceData[] instances = await GetFlowInstancesAsync(flowId);
            instances.Should().HaveCount(1);
            instances.Should().OnlyContain(found => found.State == "Complete");

            await using CoreDataContext core = CreateCoreContext();
            ScheduledTask task = await core.Set<ScheduledTask>().IgnoreQueryFilters()
                .FirstAsync(found => found.Id == taskId);

            task.LastExecuted.Should().NotBeNull();
            task.NextExecution.Should().NotBeNull();
            task.NextExecution.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
        }
        finally
        {
            await DeleteFlowArtifactsAsync(flowId, taskId);
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
            (executeOnlyUserId, executeOnlyRoleId) = await CreateExecuteOnlyUserAsync(BaselineAppId);

            flowId = await CreateFlowDefinitionAsync(BaselineAppId, Unique("Execute Only Scheduled Flow"));
            taskId = await CreateScheduledTaskAsync(
                flowId,
                Unique("Execute Only Scheduled Task"),
                nextExecution: DateTimeOffset.UtcNow.AddMinutes(-5),
                executeAs: executeOnlyUserId);

            await fixture.RestartHostedServicesAsync();

            await WaitUntilAsync(
                async () => await HasAnyFlowInstanceAsync(flowId),
                attempts: 60,
                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId));

            fixture.HostedServicesOutput.Should().NotContain("Exception thrown whilst raising scheduled_task_execute event");
            fixture.HostedServicesOutput.Should().NotContain("Access Denied!");

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId);
            instance.Should().NotBeNull();
            instance.Caller.Should().Be(executeOnlyUserId);
            instance.State.Should().NotBe("Queued");

            FlowInstanceData[] instances = await GetFlowInstancesAsync(flowId);
            instances.Should().HaveCount(1);
        }
        finally
        {
            await DeleteFlowArtifactsAsync(flowId, taskId);
            await DeleteExecuteOnlyUserAsync(executeOnlyUserId, executeOnlyRoleId);
        }
    }
}
