// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Data.Models.CMS;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace cCoder.Core.Tests.Cors;

public sealed partial class AllowedOriginStoreServiceTests
{
    [Fact]
    public async Task GetAllowedOriginsAsyncReturnsCurrentAppOrigins()
    {
        // Given
        Mock<IContentManagementAppBroker> appBrokerMock = new();
        DefaultHttpContext httpContext = new();
        httpContext.Request.Host = new HostString("app.example.com");

        App app = new()
        {
            Domain = "app.example.com",
            ConfigJson = """
                {
                    "allowedOrigins": ["https://admin.example.com"]
                }
                """,
        };

        appBrokerMock
            .Setup(
                expression: broker => broker.GetAppByDomain(
                    domain: "app.example.com",
                    ignoreFilters: true))
            .Returns(value: app);

        TestHttpRequestBroker httpRequestBroker = new(
            request: httpContext.Request);

        AllowedOriginStoreService service = new(
            appBroker: appBrokerMock.Object,
            httpRequestBroker: httpRequestBroker);

        // When
        string[] actualOrigins = await service.GetAllowedOriginsAsync();

        // Then
        actualOrigins.Should()
            .BeEquivalentTo(
                expectation:
                [
                    "app.example.com",
                    "https://admin.example.com"
                ]);
    }

    [Fact]
    public async Task GetAllowedOriginsAsyncReturnsEmptyWithoutRequest()
    {
        // Given
        Mock<IContentManagementAppBroker> appBrokerMock = new();
        TestHttpRequestBroker httpRequestBroker = new(request: null);

        AllowedOriginStoreService service = new(
            appBroker: appBrokerMock.Object,
            httpRequestBroker: httpRequestBroker);

        // When
        string[] actualOrigins = await service.GetAllowedOriginsAsync();

        // Then
        actualOrigins.Should()
            .BeEmpty();

        appBrokerMock.Verify(
            expression: broker => broker.GetAppByDomain(
                domain: It.IsAny<string>(),
                ignoreFilters: It.IsAny<bool>()),
            times: Times.Never);
    }

    [Fact]
    public async Task GetAllowedOriginsAsyncReturnsEmptyWithoutCurrentApp()
    {
        // Given
        Mock<IContentManagementAppBroker> appBrokerMock = new();
        DefaultHttpContext httpContext = new();
        httpContext.Request.Host = new HostString("missing.example.com");

        appBrokerMock
            .Setup(
                expression: broker => broker.GetAppByDomain(
                    domain: "missing.example.com",
                    ignoreFilters: true))
            .Returns(value: null);

        TestHttpRequestBroker httpRequestBroker = new(
            request: httpContext.Request);

        AllowedOriginStoreService service = new(
            appBroker: appBrokerMock.Object,
            httpRequestBroker: httpRequestBroker);

        // When
        string[] actualOrigins = await service.GetAllowedOriginsAsync();

        // Then
        actualOrigins.Should()
            .BeEmpty();
    }

    private sealed class TestHttpRequestBroker(HttpRequest request)
        : IHttpRequestBroker
    {
        public HttpRequest GetCurrentRequest() =>
            request;
    }
}