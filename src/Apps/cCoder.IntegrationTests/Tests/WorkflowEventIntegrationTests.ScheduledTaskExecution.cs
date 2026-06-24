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
}
