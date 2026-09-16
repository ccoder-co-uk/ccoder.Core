// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Eventing;
using cCoder.Core.Dependencies.Eventing;
using cCoder.Data;
using cCoder.Eventing;
using Moq;

namespace cCoder.Core.Tests;

public sealed partial class PackageImportCompletionEventBrokerTests
{
    private readonly Mock<IEventHub> eventHubMock =
        new(MockBehavior.Strict);

    private readonly Mock<ICoreAuthInfo> authInfoMock =
        new(MockBehavior.Strict);

    private PackageImportCompletionEventBroker CreateBroker() =>
        new(eventingDependency: new EventingDependency(
            eventHub: eventHubMock.Object,
            authInfo: authInfoMock.Object));
}