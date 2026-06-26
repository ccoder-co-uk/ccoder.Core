using System.Text.Json;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing;
using cCoder.Eventing.Http.Models;
using cCoder.Eventing.Models;
using FluentAssertions;
using HostedServices;
using Moq;
using cCoder.Workflow.Services.Orchestrations;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed class ReceivedHttpEventProcessorTests
{
    [Fact]
    public async Task ProcessAsync_GivenFolderDeleteMessage_ShouldRaiseInternalEvent()
    {
        Mock<IEventHub> eventHubMock = new();
        Mock<IWorkflowInstanceManagementOrchestrationService> workflowManagementServiceMock = new();
        EventMessage<Folder> actualMessage = null;
        Folder folder = new()
        {
            Id = Guid.NewGuid(),
            AppId = 42,
            Name = "content",
            Path = "content",
        };

        eventHubMock
            .Setup(eventHub => eventHub.RaiseEventAsync(
                "folder_delete",
                It.IsAny<EventMessage<Folder>>()))
            .Callback<string, EventMessage<Folder>>((_, message) => actualMessage = message)
            .Returns(ValueTask.CompletedTask);

        ReceivedHttpEventProcessor processor = new(
            eventHubMock.Object,
            workflowManagementServiceMock.Object,
            new HttpEventingOptions
            {
                JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            });

        await processor.ProcessAsync(new HttpEventMessage
        {
            EventName = "folder_delete",
            Data = JsonSerializer.Serialize(folder),
        });

        actualMessage.Should().NotBeNull();
        actualMessage.AuthInfo.Should().NotBeNull();
        actualMessage.AuthInfo.SSOUserId.Should().Be("Guest");
        actualMessage.Data.Should().NotBeNull();
        actualMessage.Data.Id.Should().Be(folder.Id);
        actualMessage.Data.AppId.Should().Be(folder.AppId);
        actualMessage.Data.Path.Should().Be(folder.Path);

        eventHubMock.Verify(eventHub => eventHub.RaiseEventAsync(
            "folder_delete",
            It.IsAny<EventMessage<Folder>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_GivenFlowInstanceDataAddMessage_ShouldExecuteQueuedWorkflow()
    {
        Mock<IEventHub> eventHubMock = new();
        Mock<IWorkflowInstanceManagementOrchestrationService> workflowManagementServiceMock = new();
        FlowInstanceData flowInstanceData = new()
        {
            Id = Guid.NewGuid(),
            FlowDefinitionId = Guid.NewGuid(),
            State = "Queued",
            Caller = "admin"
        };

        ReceivedHttpEventProcessor processor = new(
            eventHubMock.Object,
            workflowManagementServiceMock.Object,
            new HttpEventingOptions
            {
                JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            });

        await processor.ProcessAsync(new HttpEventMessage
        {
            EventName = "flow_instance_data_add",
            Data = JsonSerializer.Serialize(flowInstanceData),
        });

        workflowManagementServiceMock.Verify(service =>
            service.ExecuteWaitingQueuedInstanceByIdAsync(flowInstanceData.Id),
            Times.Once);

        eventHubMock.Verify(eventHub => eventHub.RaiseEventAsync(
                It.IsAny<string>(),
                It.IsAny<EventMessage<FlowInstanceData>>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_GivenNonQueuedFlowInstanceDataAddMessage_ShouldNotExecuteWorkflow()
    {
        Mock<IEventHub> eventHubMock = new();
        Mock<IWorkflowInstanceManagementOrchestrationService> workflowManagementServiceMock = new();
        FlowInstanceData flowInstanceData = new()
        {
            Id = Guid.NewGuid(),
            FlowDefinitionId = Guid.NewGuid(),
            State = "Executing",
            Caller = "admin"
        };

        ReceivedHttpEventProcessor processor = new(
            eventHubMock.Object,
            workflowManagementServiceMock.Object,
            new HttpEventingOptions
            {
                JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            });

        await processor.ProcessAsync(new HttpEventMessage
        {
            EventName = "flow_instance_data_add",
            Data = JsonSerializer.Serialize(flowInstanceData),
        });

        workflowManagementServiceMock.Verify(service =>
            service.ExecuteWaitingQueuedInstanceByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }
}
