using cCoder.Core.Models;
using cCoder.Core.Services.Processings.AllowedOrigins;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests.Cors;

public sealed class AllowedOriginProcessingServiceTests
{
    private readonly AllowedOriginProcessingService service = new();

    [Fact]
    public void IsAllowed_ShouldPermitLoopbackOrigins()
    {
        CoreAllowedOriginSnapshot snapshot = service.CreateSnapshot([]);

        service.IsAllowed("https://localhost:3000", snapshot).Should().BeTrue();
        service.IsAllowed("http://127.0.0.1:5173", snapshot).Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_ShouldMatchConfiguredHostWithoutScheme()
    {
        CoreAllowedOriginSnapshot snapshot = service.CreateSnapshot(["app.example.com"]);

        service.IsAllowed("https://app.example.com", snapshot).Should().BeTrue();
        service.IsAllowed("http://app.example.com", snapshot).Should().BeTrue();
        service.IsAllowed("https://other.example.com", snapshot).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldMatchConfiguredAuthorityWithPort()
    {
        CoreAllowedOriginSnapshot snapshot = service.CreateSnapshot(["app.example.com:8443"]);

        service.IsAllowed("https://app.example.com:8443", snapshot).Should().BeTrue();
        service.IsAllowed("https://app.example.com", snapshot).Should().BeFalse();
        service.IsAllowed("https://app.example.com:9443", snapshot).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldRespectConfiguredOriginSchemeWhenProvided()
    {
        CoreAllowedOriginSnapshot snapshot =
            service.CreateSnapshot(["https://secure.example.com"]);

        service.IsAllowed("https://secure.example.com", snapshot).Should().BeTrue();
        service.IsAllowed("http://secure.example.com", snapshot).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldRejectInvalidOrigins()
    {
        CoreAllowedOriginSnapshot snapshot = service.CreateSnapshot(["app.example.com"]);

        service.IsAllowed(string.Empty, snapshot).Should().BeFalse();
        service.IsAllowed("not-an-origin", snapshot).Should().BeFalse();
        service.IsAllowed("ftp://app.example.com", snapshot).Should().BeFalse();
    }
}
