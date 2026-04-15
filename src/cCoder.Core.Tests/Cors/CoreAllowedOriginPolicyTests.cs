using cCoder.Core.Cors;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests.Cors;

public sealed class CoreAllowedOriginPolicyTests
{
    [Fact]
    public void IsAllowed_ShouldPermitLoopbackOrigins()
    {
        CoreAllowedOriginSnapshot snapshot = CoreAllowedOriginPolicy.CreateSnapshot([]);

        CoreAllowedOriginPolicy.IsAllowed("https://localhost:3000", snapshot).Should().BeTrue();
        CoreAllowedOriginPolicy.IsAllowed("http://127.0.0.1:5173", snapshot).Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_ShouldMatchConfiguredHostWithoutScheme()
    {
        CoreAllowedOriginSnapshot snapshot = CoreAllowedOriginPolicy.CreateSnapshot(["app.example.com"]);

        CoreAllowedOriginPolicy.IsAllowed("https://app.example.com", snapshot).Should().BeTrue();
        CoreAllowedOriginPolicy.IsAllowed("http://app.example.com", snapshot).Should().BeTrue();
        CoreAllowedOriginPolicy.IsAllowed("https://other.example.com", snapshot).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldMatchConfiguredAuthorityWithPort()
    {
        CoreAllowedOriginSnapshot snapshot = CoreAllowedOriginPolicy.CreateSnapshot(["app.example.com:8443"]);

        CoreAllowedOriginPolicy.IsAllowed("https://app.example.com:8443", snapshot).Should().BeTrue();
        CoreAllowedOriginPolicy.IsAllowed("https://app.example.com", snapshot).Should().BeFalse();
        CoreAllowedOriginPolicy.IsAllowed("https://app.example.com:9443", snapshot).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldRespectConfiguredOriginSchemeWhenProvided()
    {
        CoreAllowedOriginSnapshot snapshot =
            CoreAllowedOriginPolicy.CreateSnapshot(["https://secure.example.com"]);

        CoreAllowedOriginPolicy.IsAllowed("https://secure.example.com", snapshot).Should().BeTrue();
        CoreAllowedOriginPolicy.IsAllowed("http://secure.example.com", snapshot).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldRejectInvalidOrigins()
    {
        CoreAllowedOriginSnapshot snapshot = CoreAllowedOriginPolicy.CreateSnapshot(["app.example.com"]);

        CoreAllowedOriginPolicy.IsAllowed(string.Empty, snapshot).Should().BeFalse();
        CoreAllowedOriginPolicy.IsAllowed("not-an-origin", snapshot).Should().BeFalse();
        CoreAllowedOriginPolicy.IsAllowed("ftp://app.example.com", snapshot).Should().BeFalse();
    }
}
