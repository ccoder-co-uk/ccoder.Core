using cCoder.Data.Models.Workflow;
using FluentAssertions;
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
}
