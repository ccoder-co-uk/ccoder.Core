// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;
using cCoder.Core.Brokers.Eventing;
using cCoder.Data.Models.Packaging;
using cCoder.Eventing.Models;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class PackageImportCompletionEventBrokerTests
{
    [Fact]
    public async Task ShouldRaiseCompletionUsingUnifiedPackageImportCompleteEvent()
    {
        // Given
        PackageImportEvent packageImportEvent = new()
        {
            AppId = 42,
            Package = new Package
            {
                Name = "App package",
                Items = [],
            },
        };

        authInfoMock
            .SetupGet(expression: authInfo => authInfo.SSOUserId)
            .Returns(value: "user-id");

        eventHubMock
            .Setup(expression: eventHub => eventHub.RaiseEventAsync(
                name: "package_import_complete",
                message: It.Is<EventMessage<PackageImportEvent>>(
                    predicate: message =>
                        message.Data == packageImportEvent
                        && message.AuthInfo.SSOUserId == "user-id")))
            .Returns(value: ValueTask.CompletedTask);

        PackageImportCompletionEventBroker broker = CreateBroker();

        // When
        await broker.RaisePackageImportEventCompleteAsync(
            packageImportEvent: packageImportEvent);

        // Then
        eventHubMock.Verify(
            expression: eventHub => eventHub.RaiseEventAsync(
                name: "package_import_complete",
                message: It.Is<EventMessage<PackageImportEvent>>(
                    predicate: message =>
                        message.Data == packageImportEvent
                        && message.AuthInfo.SSOUserId == "user-id")),
            times: Times.Once);
    }
}
