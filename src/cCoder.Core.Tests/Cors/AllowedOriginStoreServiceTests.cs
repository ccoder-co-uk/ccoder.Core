using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Exposures.Cors;
using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Core.Services.Processings.AllowedOrigins;
using cCoder.Data.Models.CMS;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace cCoder.Core.Tests.Cors;

public sealed class AllowedOriginStoreServiceTests
{
    [Fact]
    public async Task GetAllowedOriginsAsync_ShouldUseOnlyCurrentRequestAppWhenRequestExists()
    {
        Mock<IContentManagementAppBroker> appBrokerMock = new();

        DefaultHttpContext httpContext = new();
        httpContext.Request.Host = new HostString("app.example.com");

        appBrokerMock.Setup(broker => broker.GetByDomain("app.example.com", true))
            .Returns(
                new App
                {
                    Domain = "app.example.com",
                    ConfigJson = """
                    {
                        "allowedOrigins": ["https://admin.example.com", "APP.example.com"],
                        "theme": "dark"
                    }
                    """
                });

        AllowedOriginStoreService service = new(
            appBrokerMock.Object,
            new TestHttpRequestBroker(httpContext.Request));

        string[] origins = await service.GetAllowedOriginsAsync();

        origins.Should().BeEquivalentTo(
            "app.example.com",
            "https://admin.example.com");

        appBrokerMock.Verify(broker => broker.GetByDomain("app.example.com", true), Times.Once);
        appBrokerMock.Verify(broker => broker.GetAll(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetAllowedOriginsAsync_ShouldReturnEmptyWhenRequestDoesNotExist()
    {
        Mock<IContentManagementAppBroker> appBrokerMock = new();

        AllowedOriginStoreService service = new(
            appBrokerMock.Object,
            new TestHttpRequestBroker(null));

        string[] origins = await service.GetAllowedOriginsAsync();

        origins.Should().BeEmpty();
        appBrokerMock.Verify(broker => broker.GetAll(It.IsAny<bool>()), Times.Never);
        appBrokerMock.Verify(
            broker => broker.GetByDomain(It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllowedOriginsAsync_ShouldReturnEmptyWhenCurrentRequestAppDoesNotExist()
    {
        Mock<IContentManagementAppBroker> appBrokerMock = new();

        DefaultHttpContext httpContext = new();
        httpContext.Request.Host = new HostString("missing.example.com");

        appBrokerMock.Setup(broker => broker.GetByDomain("missing.example.com", true))
            .Returns((App)null);

        AllowedOriginStoreService service = new(
            appBrokerMock.Object,
            new TestHttpRequestBroker(httpContext.Request));

        string[] origins = await service.GetAllowedOriginsAsync();

        origins.Should().BeEmpty();
        appBrokerMock.Verify(broker => broker.GetAll(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task IsAllowedAsync_ShouldUseCurrentRequestAppOrigins()
    {
        Mock<IContentManagementAppBroker> appBrokerMock = new();

        DefaultHttpContext httpContext = new();
        httpContext.Request.Host = new HostString("app.example.com");

        appBrokerMock.Setup(broker => broker.GetByDomain("app.example.com", true))
            .Returns(
                new App
                {
                    Domain = "app.example.com",
                    ConfigJson = """
                    {
                        "allowedOrigins": ["https://admin.example.com"]
                    }
                    """
                });

        using ServiceProvider serviceProvider = new ServiceCollection()
            .AddTransient(_ => appBrokerMock.Object)
            .AddTransient<IHttpRequestBroker>(_ => new TestHttpRequestBroker(httpContext.Request))
            .AddTransient<IAllowedOriginStoreService, AllowedOriginStoreService>()
            .AddTransient<IAllowedOriginProcessingService, AllowedOriginProcessingService>()
            .AddLogging()
            .BuildServiceProvider();

        CoreAllowedOriginStore store = new(
            serviceProvider.GetRequiredService<IAllowedOriginStoreService>(),
            serviceProvider.GetRequiredService<IAllowedOriginProcessingService>(),
            serviceProvider.GetRequiredService<ILogger<CoreAllowedOriginStore>>());

        (await store.IsAllowedAsync("https://admin.example.com")).Should().BeTrue();
        (await store.IsAllowedAsync("https://other.example.com")).Should().BeFalse();

        appBrokerMock.Verify(broker => broker.GetByDomain("app.example.com", true), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAllowedOriginsAsync_ShouldNotIncludeOtherTenantOriginsWhenRequestExists()
    {
        Mock<IContentManagementAppBroker> appBrokerMock = new();

        DefaultHttpContext httpContext = new();
        httpContext.Request.Host = new HostString("app.example.com");

        appBrokerMock.Setup(broker => broker.GetByDomain("app.example.com", true))
            .Returns(
                new App
                {
                    Domain = "app.example.com",
                    ConfigJson = """
                    {
                        "allowedOrigins": ["https://admin.example.com"]
                    }
                    """
                });

        AllowedOriginStoreService service = new(
            appBrokerMock.Object,
            new TestHttpRequestBroker(httpContext.Request));

        string[] origins = await service.GetAllowedOriginsAsync();

        origins.Should().NotContain("other.example.com");
        origins.Should().NotContain("https://other-admin.example.com");
    }

    [Fact]
    public void ExtractOriginsFromConfigJson_ShouldExtractKnownOriginShapes()
    {
        const string configJson = """
        {
            "allowedOrigins": ["https://admin.example.com", "portal.example.com:8443"],
            "nested": {
                "apiUrl": "https://api.example.com/v1",
                "theme": "dark"
            },
            "supportDomains": [
                { "host": "support.example.com" }
            ]
        }
        """;

        string[] origins = AllowedOriginStoreService
            .ExtractOriginsFromConfigJson(configJson)
            .ToArray();

        origins.Should().BeEquivalentTo(
            "https://admin.example.com",
            "portal.example.com:8443",
            "https://api.example.com/v1",
            "support.example.com");
    }

    [Fact]
    public void ExtractOriginsFromConfigJson_ShouldIgnoreMalformedJson()
    {
        string[] origins = AllowedOriginStoreService
            .ExtractOriginsFromConfigJson("{not json")
            .ToArray();

        origins.Should().BeEmpty();
    }

    private sealed class TestHttpRequestBroker(HttpRequest request) : IHttpRequestBroker
    {
        public HttpRequest GetCurrentRequest() => request;
    }
}
