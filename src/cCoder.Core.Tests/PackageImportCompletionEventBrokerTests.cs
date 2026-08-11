// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Eventing;
using cCoder.Eventing;
using Moq;

namespace cCoder.Core.Tests;

public sealed partial class PackageImportCompletionEventBrokerTests
{
    private readonly Mock<IEventHub> eventHubMock =
        new(MockBehavior.Strict);

    private PackageImportCompletionEventBroker CreateBroker() =>
        new(eventHub: eventHubMock.Object);
}