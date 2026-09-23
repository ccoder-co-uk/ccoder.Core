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
        Mock<IContentManagementAppPackageProcessingService> contentManagementAppPackageProcessingServiceMock = new();
        Mock<IAppSecurityPackageProcessingService> appSecurityPackageProcessingServiceMock = new();
        Mock<IContentManagementPackageProcessingService> contentManagementPackageProcessingServiceMock = new();
        Mock<IDocumentManagementPackageProcessingService> documentManagementPackageProcessingServiceMock = new();
        Mock<ISchedulingPackageProcessingService> schedulingPackageProcessingServiceMock = new();
        Mock<IWorkflowPackageProcessingService> workflowPackageProcessingServiceMock = new();

        contentManagementAppPackageProcessingServiceMock
            .Setup(expression: service => service.ExportAppConfigurationAsync(
                appId: 42,
                sourceApi: "source"))
            .ReturnsAsync(value: new Package { Name = "AppConfiguration" });

        documentManagementPackageProcessingServiceMock
            .Setup(expression: service => service.ExportPackage(
                appId: 42,
                packageName: "FolderRoles"))
            .Returns(value: new Package { Name = "FolderRoles" });

        contentManagementPackageProcessingServiceMock
            .Setup(expression: service => service.ExportPackage(
                appId: 42,
                packageName: "Pages"))
            .Returns(value: new Package { Name = "Pages" });

        contentManagementPackageProcessingServiceMock
            .Setup(expression: service => service.ExportPackage(
                appId: 42,
                packageName: "PAGEROLES"))
            .Returns(value: new Package { Name = "PageRoles" });

        PackageManagerAggregationService service = new(
            contentManagementAppPackageProcessingService:
                contentManagementAppPackageProcessingServiceMock.Object,
            appSecurityPackageProcessingService: appSecurityPackageProcessingServiceMock.Object,
            contentManagementPackageProcessingService: contentManagementPackageProcessingServiceMock.Object,
            documentManagementPackageProcessingService: documentManagementPackageProcessingServiceMock.Object,
            schedulingPackageProcessingService: schedulingPackageProcessingServiceMock.Object,
            workflowPackageProcessingService: workflowPackageProcessingServiceMock.Object);

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