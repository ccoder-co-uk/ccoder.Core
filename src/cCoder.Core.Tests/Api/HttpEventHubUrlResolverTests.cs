// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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

        string result = HttpEventHubUrlResolver.Resolve(configuration: configuration);

        result.Should()
            .Be(expected: "https://hosted.local/Api/Eventing");
    }

    [Fact]
    public void Resolve_ShouldPreserveExplicitHubUrl()
    {
        IConfiguration configuration = BuildConfiguration(
values:             ("Eventing:Http:HubUrl", "https://hosted.local/Api/Eventing"));

        string result = HttpEventHubUrlResolver.Resolve(configuration: configuration);

        result.Should()
            .Be(expected: "https://hosted.local/Api/Eventing");
    }

    [Fact]
    public void Resolve_ShouldReturnEmptyWhenExternalEventingIsDisabled()
    {
        IConfiguration configuration = BuildConfiguration(
            ("Settings:enableExternalEventing", "false"),
            ("Services:HostedServices", "https://hosted.local"));

        string result = HttpEventHubUrlResolver.Resolve(configuration: configuration);

        result.Should()
            .BeEmpty();
    }

    [Fact]
    public void Normalize_ShouldLeaveNonRootAbsolutePathsUntouched()
    {
        string result = HttpEventHubUrlResolver.Normalize(value: "https://hosted.local/internal/event-hub");

        result.Should()
            .Be(expected: "https://hosted.local/internal/event-hub");
    }

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: values.ToDictionary(keySelector: item => item.Key,elementSelector: item => item.Value))
            .Build();
}