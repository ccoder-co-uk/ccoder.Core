// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Workflow;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class WorkflowEventIntegrationTests
{
    [Fact]
    public async Task ManualFlowExecution_QueuesAndCompletesWorkflowInstance()
    {
        // Given
        Guid flowId = Guid.Empty;

        try
        {
            flowId = await CreateFlowDefinitionAsync(appId: BaselineAppId,name: Unique(prefix: "Manual Flow"));
            string authToken = await CreateAuthTokenAsync(userId: AdminUserId);

            // When
            await PostRawAsync(relativeUrl: $"/Api/Workflow/FlowDefinition({flowId})/Execute?t={authToken}",body: "{}");

            await WaitUntilAsync(predicate: async () => await HasAnyFlowInstanceAsync(flowId: flowId));

            await WaitUntilAsync(
predicate:                 async () => await HasFlowInstanceStateAsync(flowId: flowId,state: "Complete"),                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId: flowId);

            // Then
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
            await DeleteFlowArtifactsAsync(flowId: flowId);
        }
    }
}