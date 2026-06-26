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
            .Setup(broker => broker.FlushOldInstancesAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        brokerMock
            .Setup(broker => broker.RequeueHungExecutingInstancesAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        brokerMock
            .Setup(broker => broker.GetQueuedInstances())
            .Returns(
            [
                new FlowInstanceData { Id = firstQueuedInstanceId, State = "Queued" },
                new FlowInstanceData { Id = firstQueuedInstanceId, State = "Queued" },
                new FlowInstanceData { Id = secondQueuedInstanceId, State = "Queued" },
            ]);

        brokerMock
            .Setup(broker => broker.ClaimQueuedInstanceAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlowInstanceData)null);

        HostedServicesWorkflowInstanceManagementOrchestrationService service = new(
            brokerMock.Object,
            Mock.Of<ICoreContextFactory>(),
            Mock.Of<IServiceProvider>(),
            new ConfigurationBuilder().Build(),
            NullLogger<HostedServicesWorkflowInstanceManagementOrchestrationService>.Instance);

        await service.RunAsync();

        brokerMock.Verify(
            broker => broker.RequeueHungExecutingInstancesAsync(
                It.Is<DateTimeOffset>(cutoff => cutoff < DateTimeOffset.UtcNow.AddMinutes(-29)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        brokerMock.Verify(
            broker => broker.GetQueuedInstances(),
            Times.Once);

        brokerMock.Verify(
            broker => broker.ClaimQueuedInstanceAsync(firstQueuedInstanceId, It.IsAny<CancellationToken>()),
            Times.Once);

        brokerMock.Verify(
            broker => broker.ClaimQueuedInstanceAsync(secondQueuedInstanceId, It.IsAny<CancellationToken>()),
            Times.Once);

        brokerMock.Invocations
            .Count(invocation => invocation.Method.Name == nameof(IWorkflowInstanceManagementBroker.ClaimQueuedInstanceAsync))
            .Should()
            .Be(2);
    }
}
