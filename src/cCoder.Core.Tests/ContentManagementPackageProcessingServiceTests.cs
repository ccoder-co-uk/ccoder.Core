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

public sealed class ContentManagementPackageProcessingServiceTests
{
    [Fact]
    public async Task ImportPackageAsync_WhenAppIdIsNull_ForwardsCommonCacheImportUnchanged()
    {
        Package package = new() { Name = "Common Cache" };
        Mock<IContentManagementPackageService> packageServiceMock = new();
        packageServiceMock.Setup(service => service.ImportPackageAsync(null, package))
            .Returns(ValueTask.CompletedTask);

        ContentManagementPackageProcessingService service = new(
            contentManagementPackageService: packageServiceMock.Object);

        await service.ImportPackageAsync(appId: null, package: package);

        packageServiceMock.Verify(
            expression: service => service.ImportPackageAsync(null, package),
            times: Times.Once);
    }

    [Fact]
    public void ExportPackage_WhenFoundationReturnsPackage_ReturnsSamePackage()
    {
        Package expected = new() { Name = "Pages" };
        Mock<IContentManagementPackageService> packageServiceMock = new();
        packageServiceMock.Setup(service => service.ExportPackage(42, "Pages"))
            .Returns(expected);

        ContentManagementPackageProcessingService service = new(
            contentManagementPackageService: packageServiceMock.Object);

        Package actual = service.ExportPackage(appId: 42, packageName: "Pages");

        actual.Should().BeSameAs(expected);
    }
}