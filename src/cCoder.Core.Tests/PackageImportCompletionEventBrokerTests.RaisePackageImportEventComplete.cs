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
        EventMessage<PackageImportEvent> message = new()
        {
            Data = new PackageImportEvent
            {
                AppId = 42,
                Package = new Package
                {
                    Name = "App package",
                    Items = [],
                },
            },
        };

        eventHubMock
            .Setup(expression: eventHub => eventHub.RaiseEventAsync(
                name: "package_import_complete",
                message: message))
            .Returns(value: ValueTask.CompletedTask);

        PackageImportCompletionEventBroker broker = CreateBroker();

        // When
        await broker.RaisePackageImportEventCompleteAsync(message: message);

        // Then
        eventHubMock.Verify(
            expression: eventHub => eventHub.RaiseEventAsync(
                name: "package_import_complete",
                message: message),
            times: Times.Once);
    }
}
