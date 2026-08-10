// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using cCoder.Data;
using cCoder.Data.Models.Workflow;
using cCoder.Core.Models;
using cCoder.Workflow.Brokers;
using cCoder.Workflow.Exposures;
using FluentAssertions;
using HostedServices;
using Moq;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed partial class HostedServicesWorkflowInstanceManagementOrchestrationServiceTests
{
    [Fact]
    public async Task RunAsync_ShouldRequeueHungInstancesAndOnlyClaimDistinctQueuedInstanceIds()
    {
        // Given
        Guid firstQueuedInstanceId = Guid.NewGuid();
        Guid secondQueuedInstanceId = Guid.NewGuid();
        Mock<IWorkflowInstanceManagementBroker> brokerMock = new();

        brokerMock
            .Setup(expression: broker => broker.FlushOldInstancesAsync(
                cutoff: It.IsAny<DateTimeOffset>(),
                cancellationToken: It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: 0);

        brokerMock
            .Setup(expression: broker => broker.RequeueHungExecutingInstancesAsync(
                cutoff: It.IsAny<DateTimeOffset>(),
                cancellationToken: It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: 1);

        brokerMock
            .Setup(expression: broker => broker.GetQueuedInstances())
            .Returns(
                value:
                [
                new FlowInstanceData { Id = firstQueuedInstanceId, State = "Queued" },
                new FlowInstanceData { Id = firstQueuedInstanceId, State = "Queued" },
                new FlowInstanceData { Id = secondQueuedInstanceId, State = "Queued" },
            ]);

        brokerMock
            .Setup(expression: broker => broker.UpdateQueuedInstanceClaimAsync(
                flowInstanceDataId: It.IsAny<Guid>(),
                cancellationToken: It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: 0);

        HostedServicesWorkflowInstanceManagementOrchestrationService service = new(
            brokerMock.Object,
            Mock.Of<IFlowInstanceDataManager>(),
            Mock.Of<ICoreContextFactory>(),
            Mock.Of<IServiceProvider>(),
            CoreConfigurationFactory.Create(),
            Mock.Of<ILoggingBroker>());

        // When
        await service.RunAsync();

        // Then
        brokerMock.Verify(
            expression: broker => broker.RequeueHungExecutingInstancesAsync(
                cutoff: It.Is<DateTimeOffset>(
                    match: cutoff =>
                        cutoff < DateTimeOffset.UtcNow.AddMinutes(minutes: -29)),
                cancellationToken: It.IsAny<CancellationToken>()),
            times: Times.Once);

        brokerMock.Verify(
            expression: broker => broker.GetQueuedInstances(),
            times: Times.Once);

        brokerMock.Verify(
            expression: broker => broker.UpdateQueuedInstanceClaimAsync(
                flowInstanceDataId: firstQueuedInstanceId,
                cancellationToken: It.IsAny<CancellationToken>()),
            times: Times.Once);

        brokerMock.Verify(
            expression: broker => broker.UpdateQueuedInstanceClaimAsync(
                flowInstanceDataId: secondQueuedInstanceId,
                cancellationToken: It.IsAny<CancellationToken>()),
            times: Times.Once);

        brokerMock.Invocations
            .Count(predicate: invocation =>
                invocation.Method.Name == nameof(
                    IWorkflowInstanceManagementBroker.UpdateQueuedInstanceClaimAsync))
            .Should()
            .Be(expected: 2);
    }
}