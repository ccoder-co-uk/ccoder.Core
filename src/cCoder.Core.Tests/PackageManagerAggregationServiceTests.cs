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

public sealed class PackageManagerAggregationServiceTests
{
    [Fact]
    public async Task ExportPackagesAsync_WhenSpecialAndDomainNamesRequested_PreservesRequestOrder()
    {
        Mock<ICorePackageProcessingService> corePackageProcessingServiceMock = new();

        corePackageProcessingServiceMock
            .Setup(service => service.ExportAppConfigurationAsync(42, "source"))
            .ReturnsAsync(new Package { Name = "AppConfiguration" });
        corePackageProcessingServiceMock
            .Setup(service => service.ExportPageRolesAsync(42, "source"))
            .ReturnsAsync(new Package { Name = "PageRoles" });
        corePackageProcessingServiceMock
            .Setup(service => service.ExportFolderRolesAsync(42, "source"))
            .ReturnsAsync(new Package { Name = "FolderRoles" });
        corePackageProcessingServiceMock
            .Setup(service => service.ExportPackage(42, "Pages"))
            .Returns(new Package { Name = "Pages" });

        PackageManagerAggregationService service = new(
            corePackageProcessingService: corePackageProcessingServiceMock.Object);

        Package[] packages = await service.ExportPackagesAsync(
            appId: 42,
            packageNames: ["FolderRoles", "Pages", "appconfiguration", "PAGEROLES"],
            sourceApi: "source");

        packages.Select(selector: package => package.Name)
            .Should().Equal("FolderRoles", "Pages", "AppConfiguration", "PageRoles");
    }
}