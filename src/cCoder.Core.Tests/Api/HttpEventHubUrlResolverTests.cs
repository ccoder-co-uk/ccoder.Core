using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed class HttpEventHubUrlResolverTests
{
    [Fact]
    public void Resolve_ShouldAppendDefaultEventingPathForHostedServicesRoot()
    {
        IConfiguration configuration = BuildConfiguration(
            ("Settings:enableExternalEventing", "true"),
            ("Services:HostedServices", "https://hosted.local"));

        string result = HttpEventHubUrlResolver.Resolve(configuration);

        result.Should().Be("https://hosted.local/Api/Eventing");
    }

    [Fact]
    public void Resolve_ShouldPreserveExplicitHubUrl()
    {
        IConfiguration configuration = BuildConfiguration(
            ("Eventing:Http:HubUrl", "https://hosted.local/Api/Eventing"));

        string result = HttpEventHubUrlResolver.Resolve(configuration);

        result.Should().Be("https://hosted.local/Api/Eventing");
    }

    [Fact]
    public void Resolve_ShouldReturnEmptyWhenExternalEventingIsDisabled()
    {
        IConfiguration configuration = BuildConfiguration(
            ("Settings:enableExternalEventing", "false"),
            ("Services:HostedServices", "https://hosted.local"));

        string result = HttpEventHubUrlResolver.Resolve(configuration);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Normalize_ShouldLeaveNonRootAbsolutePathsUntouched()
    {
        string result = HttpEventHubUrlResolver.Normalize("https://hosted.local/internal/event-hub");

        result.Should().Be("https://hosted.local/internal/event-hub");
    }

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(item => item.Key, item => item.Value))
            .Build();
}
