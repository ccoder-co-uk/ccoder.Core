// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Exposures.PackageManagers;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Services.Aggregations.Packages;
using cCoder.Data.Models.Packaging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class PackageTransferManagerTests
{
    [Fact]
    public void GetRequestDomain_WhenRequestIsAvailable_ReturnsRequestHost()
    {
        // Given
        Mock<IPackageManagerAggregationService> packageManagerAggregationServiceMock = new();
        Mock<IHttpRequestBroker> httpRequestBrokerMock = new();
        DefaultHttpContext httpContext = new();
        httpContext.Request.Host = new HostString(value: "example.ccoder.co.uk");

        httpRequestBrokerMock
            .Setup(expression: broker => broker.GetCurrentRequest())
            .Returns(value: httpContext.Request);

        PackageTransferManager manager = new(
            packageManagerAggregationService: packageManagerAggregationServiceMock.Object,
            httpRequestBroker: httpRequestBrokerMock.Object);

        // When
        string domain = manager.GetRequestDomain();

        // Then
        domain
            .Should()
            .Be(expected: "example.ccoder.co.uk");
    }

    [Fact]
    public async Task ExportPackagesAsync_WhenRequested_DelegatesToCoreAggregation()
    {
        // Given
        string[] packageNames = ["Pages"];
        Package[] expectedPackages = [new() { Name = "Pages" }];
        Mock<IPackageManagerAggregationService> packageManagerAggregationServiceMock = new();

        packageManagerAggregationServiceMock
            .Setup(expression: service => service.ExportPackagesAsync(
                appId: 42,
                packageNames: packageNames,
                sourceApi: "https://example.ccoder.co.uk:443/Api/"))
            .ReturnsAsync(value: expectedPackages);

        PackageTransferManager manager = new(
            packageManagerAggregationService: packageManagerAggregationServiceMock.Object,
            httpRequestBroker: Mock.Of<IHttpRequestBroker>());

        // When
        Package[] actualPackages = await manager.ExportPackagesAsync(
            appId: 42,
            packageNames: packageNames,
            sourceApi: "https://example.ccoder.co.uk:443/Api/");

        // Then
        actualPackages
            .Should()
            .BeSameAs(expected: expectedPackages);
    }
}