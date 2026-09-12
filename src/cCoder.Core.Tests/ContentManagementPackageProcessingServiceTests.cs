// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.Packages;
using cCoder.Core.Services.Processings.Packages;
using cCoder.Data.Models.Packaging;
using FluentAssertions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class ContentManagementPackageProcessingServiceTests
{
    [Fact]
    public async Task ImportPackageAsync_WhenAppIdIsNull_ForwardsCommonCacheImportUnchanged()
    {
        // Given
        Package package = new() { Name = "Common Cache" };
        Mock<IContentManagementPackageService> packageServiceMock = new();

        packageServiceMock
            .Setup(expression: service => service.ImportPackageAsync(
                appId: null,
                package: package))
            .Returns(value: ValueTask.CompletedTask);

        ContentManagementPackageProcessingService service = new(
            contentManagementPackageService: packageServiceMock.Object);

        // When
        await service.ImportPackageAsync(
            appId: null,
            package: package);

        // Then
        packageServiceMock.Verify(
            expression: service => service.ImportPackageAsync(
                appId: null,
                package: package),
            times: Times.Once);
    }

    [Fact]
    public void ExportPackage_WhenFoundationReturnsPackage_ReturnsSamePackage()
    {
        // Given
        Package expected = new() { Name = "Pages" };
        Mock<IContentManagementPackageService> packageServiceMock = new();

        packageServiceMock
            .Setup(expression: service => service.ExportPackage(
                appId: 42,
                packageName: "Pages"))
            .Returns(value: expected);

        ContentManagementPackageProcessingService service = new(
            contentManagementPackageService: packageServiceMock.Object);

        // When
        Package actual = service.ExportPackage(
            appId: 42,
            packageName: "Pages");

        // Then
        actual
            .Should()
            .BeSameAs(expected: expected);
    }
}