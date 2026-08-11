// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;
using cCoder.Core.Services.Aggregations.Packages;
using cCoder.Packaging.Models;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class PackageImportAggregationServiceTests
{
    [Fact]
    public async Task HandlePackageImportAsyncShouldRouteCommonCacheOnlyToContentManagement()
    {
        // Given
        Package package = new()
        {
            Name = "Common cache",
            Items = [],
        };

        contentManagementPackageProcessingServiceMock
            .Setup(expression: service => service.ImportPackageAsync(
                appId: null,
                package: package))
            .Returns(value: ValueTask.CompletedTask);

        PackageImportEvent packageImportEvent = new()
        {
            AppId = null,
            Package = package,
        };

        packageImportCompletionEventProcessingServiceMock
            .Setup(expression: service => service
                .ProcessPackageImportEventAsync(
                    packageImportEvent: packageImportEvent))
            .Returns(value: ValueTask.CompletedTask);

        PackageImportAggregationService service = CreateService();

        // When
        await service.ProcessPackageImportEventAsync(
            packageImportEvent: packageImportEvent);

        // Then
        contentManagementPackageProcessingServiceMock.Verify(
            expression: dependency => dependency.ImportPackageAsync(
                appId: null,
                package: package),
            times: Times.Once);

        packageImportCompletionEventProcessingServiceMock.Verify(
            expression: dependency => dependency
                .ProcessPackageImportEventAsync(
                    packageImportEvent: packageImportEvent),
            times: Times.Once);

        corePackageProcessingServiceMock.VerifyNoOtherCalls();
        appSecurityPackageProcessingServiceMock.VerifyNoOtherCalls();
        documentManagementPackageProcessingServiceMock.VerifyNoOtherCalls();
        workflowPackageProcessingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandlePackageImportAsyncShouldRouteAppPackageToDomainsInOrder()
    {
        // Given
        const int appId = 42;

        Package package = new()
        {
            Name = "App package",
            Items = [],
        };

        MockSequence sequence = new();

        PackageImportEvent packageImportEvent = new()
        {
            AppId = appId,
            Package = package,
        };

        corePackageProcessingServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.ImportPackageAsync(
                appId: appId,
                package: package))
            .Returns(value: ValueTask.CompletedTask);

        contentManagementPackageProcessingServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.ImportPackageAsync(
                appId: appId,
                package: package))
            .Returns(value: ValueTask.CompletedTask);

        appSecurityPackageProcessingServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.ImportPackageAsync(
                appId: appId,
                package: package))
            .Returns(value: ValueTask.CompletedTask);

        documentManagementPackageProcessingServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.ImportPackageAsync(
                appId: appId,
                package: package))
            .Returns(value: ValueTask.CompletedTask);

        workflowPackageProcessingServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.ImportPackageAsync(
                appId: appId,
                package: package))
            .Returns(value: ValueTask.CompletedTask);

        packageImportCompletionEventProcessingServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service
                .ProcessPackageImportEventAsync(
                    packageImportEvent: packageImportEvent))
            .Returns(value: ValueTask.CompletedTask);

        PackageImportAggregationService service = CreateService();

        // When
        await service.ProcessPackageImportEventAsync(
            packageImportEvent: packageImportEvent);

        // Then
        appSecurityPackageProcessingServiceMock.VerifyAll();
        contentManagementPackageProcessingServiceMock.VerifyAll();
        documentManagementPackageProcessingServiceMock.VerifyAll();
        workflowPackageProcessingServiceMock.VerifyAll();
        corePackageProcessingServiceMock.VerifyAll();
        packageImportCompletionEventProcessingServiceMock.VerifyAll();
    }
}
