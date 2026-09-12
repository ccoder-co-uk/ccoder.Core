// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Aggregations.Packages;
using cCoder.Core.Services.Processings.Packages;
using cCoder.Data.Models.Packaging;
using FluentAssertions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class PackageManagerAggregationServiceTests
{
    [Fact]
    public async Task ExportPackagesAsync_WhenSpecialAndDomainNamesRequested_PreservesRequestOrder()
    {
        // Given
        Mock<ICorePackageProcessingService> corePackageProcessingServiceMock = new();

        corePackageProcessingServiceMock
            .Setup(expression: service => service.ExportAppConfigurationAsync(
                appId: 42,
                sourceApi: "source"))
            .ReturnsAsync(value: new Package { Name = "AppConfiguration" });

        corePackageProcessingServiceMock
            .Setup(expression: service => service.ExportPageRolesAsync(
                appId: 42,
                sourceApi: "source"))
            .ReturnsAsync(value: new Package { Name = "PageRoles" });

        corePackageProcessingServiceMock
            .Setup(expression: service => service.ExportFolderRolesAsync(
                appId: 42,
                sourceApi: "source"))
            .ReturnsAsync(value: new Package { Name = "FolderRoles" });

        corePackageProcessingServiceMock
            .Setup(expression: service => service.ExportPackage(
                appId: 42,
                packageName: "Pages"))
            .Returns(value: new Package { Name = "Pages" });

        PackageManagerAggregationService service = new(
            corePackageProcessingService: corePackageProcessingServiceMock.Object);

        // When
        Package[] packages = await service.ExportPackagesAsync(
            appId: 42,
            packageNames: ["FolderRoles", "Pages", "appconfiguration", "PAGEROLES"],
            sourceApi: "source");

        // Then
        string[] packageNames = packages
            .Select(selector: package => package.Name)
            .ToArray();

        packageNames.Length
            .Should()
            .Be(expected: 4);

        packageNames[0]
            .Should()
            .Be(expected: "FolderRoles");

        packageNames[1]
            .Should()
            .Be(expected: "Pages");

        packageNames[2]
            .Should()
            .Be(expected: "AppConfiguration");

        packageNames[3]
            .Should()
            .Be(expected: "PageRoles");
    }
}