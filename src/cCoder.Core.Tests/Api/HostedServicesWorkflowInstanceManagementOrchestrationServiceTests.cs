// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models.Workflow;
using cCoder.Workflow.Brokers;
using FluentAssertions;
using HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed class HostedServicesWorkflowInstanceManagementOrchestrationServiceTests
{
    [Fact]
    public async Task RunAsync_ShouldRequeueHungInstancesAndOnlyClaimDistinctQueuedInstanceIds()
    {
        Guid firstQueuedInstanceId = Guid.NewGuid();
        Guid secondQueuedInstanceId = Guid.NewGuid();
        Mock<IWorkflowInstanceManagementBroker> brokerMock = new();

        brokerMock
            .Setup(expression: broker => broker.FlushOldInstancesAsync(
cutoff:                 It.IsAny<DateTimeOffset>(),cancellationToken:                 It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: 0);

        brokerMock
            .Setup(expression: broker => broker.RequeueHungExecutingInstancesAsync(
cutoff:                 It.IsAny<DateTimeOffset>(),cancellationToken:                 It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: 1);

        brokerMock
            .Setup(expression: broker => broker.GetQueuedInstances())
            .Returns(
value:             [
                new FlowInstanceData { Id = firstQueuedInstanceId, State = "Queued" },
                new FlowInstanceData { Id = firstQueuedInstanceId, State = "Queued" },
                new FlowInstanceData { Id = secondQueuedInstanceId, State = "Queued" },
            ]);

        brokerMock
            .Setup(expression: broker => broker.UpdateQueuedInstanceClaimAsync(
flowInstanceDataId:                 It.IsAny<Guid>(),cancellationToken:                 It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: 0);

        HostedServicesWorkflowInstanceManagementOrchestrationService service = new(
            brokerMock.Object,
            Mock.Of<ICoreContextFactory>(),
            Mock.Of<IServiceProvider>(),
            new ConfigurationBuilder().Build(),
            NullLogger<HostedServicesWorkflowInstanceManagementOrchestrationService>.Instance);

        await service.RunAsync();

        brokerMock.Verify(
expression:             broker => broker.RequeueHungExecutingInstancesAsync(
cutoff:                 It.Is<DateTimeOffset>(match: cutoff => cutoff < DateTimeOffset.UtcNow.AddMinutes(minutes: -29)),cancellationToken:                 It.IsAny<CancellationToken>()),times:             Times.Once);

        brokerMock.Verify(
expression:             broker => broker.GetQueuedInstances(),times:             Times.Once);

        brokerMock.Verify(
expression:             broker => broker.UpdateQueuedInstanceClaimAsync(
flowInstanceDataId:                 firstQueuedInstanceId,cancellationToken:                 It.IsAny<CancellationToken>()),times:             Times.Once);

        brokerMock.Verify(
expression:             broker => broker.UpdateQueuedInstanceClaimAsync(
flowInstanceDataId:                 secondQueuedInstanceId,cancellationToken:                 It.IsAny<CancellationToken>()),times:             Times.Once);

        brokerMock.Invocations
            .Count(predicate: invocation =>
                invocation.Method.Name == nameof(
                    IWorkflowInstanceManagementBroker.UpdateQueuedInstanceClaimAsync))
            .Should()
            .Be(expected: 2);
    }
}