// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Workflow;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class FolderEventIntegrationTests
{
    [Fact]
    public async Task FolderDelete_RaisesExternalEventAndCompletesSubscribedWorkflow()
    {
        Guid flowId = Guid.Empty;
        Guid workflowEventId = Guid.Empty;
        Guid folderId = Guid.Empty;

        try
        {
            string authToken = await CreateAuthTokenAsync(AdminUserId);
            flowId = await CreateFlowDefinitionAsync(BaselineAppId, Unique("Folder Delete Flow"), authToken);
            string folderName = Unique("flow-folder");
            folderId = await CreateFolderAsync(BaselineAppId, folderName);
            workflowEventId = await CreateWorkflowEventAsync(flowId, $"folder_delete{folderName}", authToken);

            await SendWithOptionalHostAsync(HttpMethod.Delete, $"/Api/DocumentManagement/Folder({folderId})", authToken: authToken);

            await WaitUntilAsync(async () => await HasAnyFlowInstanceAsync(flowId));

            await WaitUntilAsync(
                async () => await HasFlowInstanceStateAsync(flowId, "Complete"),
                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId));

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId);
            instance.Caller.Should().Be(AdminUserId);
            instance.State.Should().Be("Complete");
            instance.ContextString.Should().Contain("Execution complete.");
        }
        finally
        {
            await DeleteWorkflowEventAsync(workflowEventId);
            await DeleteFlowArtifactsAsync(flowId);
        }
    }
}