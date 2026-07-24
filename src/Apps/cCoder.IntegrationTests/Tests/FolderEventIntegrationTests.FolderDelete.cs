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
            string authToken = await CreateAuthTokenAsync(userId: AdminUserId);
            flowId = await CreateFlowDefinitionAsync(appId: BaselineAppId,name: Unique(prefix: "Folder Delete Flow"),authToken: authToken);
            string folderName = Unique(prefix: "flow-folder");
            folderId = await CreateFolderAsync(appId: BaselineAppId,name: folderName);
            workflowEventId = await CreateWorkflowEventAsync(flowId: flowId,eventContext: $"folder_delete{folderName}",authToken: authToken);

            await SendWithOptionalHostAsync(method: HttpMethod.Delete,relativeUrl: $"/Api/DocumentManagement/Folder({folderId})",authToken: authToken);

            await WaitUntilAsync(predicate: async () => await HasAnyFlowInstanceAsync(flowId: flowId));

            await WaitUntilAsync(
predicate:                 async () => await HasFlowInstanceStateAsync(flowId: flowId,state: "Complete"),                diagnosticsFactory: () => BuildFlowDiagnosticsAsync(flowId: flowId));

            FlowInstanceData instance = await GetLatestInstanceAsync(flowId: flowId);

            instance.Caller.Should()
                .Be(expected: AdminUserId);

            instance.State.Should()
                .Be(expected: "Complete");

            instance.ContextString.Should()
                .Contain(expected: "Execution complete.");
        }
        finally
        {
            await DeleteWorkflowEventAsync(workflowEventId: workflowEventId);
            await DeleteFlowArtifactsAsync(flowId: flowId);
        }
    }
}