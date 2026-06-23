using cCoder.Data.Models.Workflow;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class WorkflowEventIntegrationTests
{
    [Fact]
    public async Task ManualFlowExecution_QueuesAndCompletesWorkflowInstance()
    {
        Guid flowId = Guid.Empty;

        try
        {
            flowId = await CreateFlowDefinitionAsync(BaselineAppId, Unique("Manual Flow"));
            string authToken = await CreateAuthTokenAsync(AdminUserId);

            await PostRawAsync($"/Api/Core/FlowDefinition({flowId})/Execute?t={authToken}", "{}");

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
            await DeleteFlowArtifactsAsync(flowId);
        }
    }
}
