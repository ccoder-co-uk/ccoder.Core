// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed partial class HttpEventHubUrlResolverTests
{
    [Fact]
    public void Resolve_ShouldAppendDefaultEventingPathForHostedServicesRoot()
    {
        // Given
        IConfiguration configuration = BuildConfiguration(
            values:
            [
                ("Settings:enableExternalEventing", "true"),
                ("Services:HostedServices", "https://hosted.local")
            ]);

        // When
        string result = HttpEventHubUrlResolver.Resolve(configuration: configuration);

        // Then
        result.Should()
            .Be(expected: "https://hosted.local/Api/Eventing");
    }

    [Fact]
    public void Resolve_ShouldPreserveExplicitHubUrl()
    {
        // Given
        IConfiguration configuration = BuildConfiguration(
            values:
            [
                ("Eventing:Http:HubUrl", "https://hosted.local/Api/Eventing")
            ]);

        // When
        string result = HttpEventHubUrlResolver.Resolve(configuration: configuration);

        // Then
        result.Should()
            .Be(expected: "https://hosted.local/Api/Eventing");
    }

    [Fact]
    public void Resolve_ShouldReturnEmptyWhenExternalEventingIsDisabled()
    {
        // Given
        IConfiguration configuration = BuildConfiguration(
            values:
            [
                ("Settings:enableExternalEventing", "false"),
                ("Services:HostedServices", "https://hosted.local")
            ]);

        // When
        string result = HttpEventHubUrlResolver.Resolve(configuration: configuration);

        // Then
        result.Should()
            .BeEmpty();
    }

    [Fact]
    public void Normalize_ShouldLeaveNonRootAbsolutePathsUntouched()
    {
        // Given
        const string eventHubUrl = "https://hosted.local/internal/event-hub";

        // When
        string result = HttpEventHubUrlResolver.Normalize(value: eventHubUrl);

        // Then
        result.Should()
            .Be(expected: "https://hosted.local/internal/event-hub");
    }

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                initialData: values.ToDictionary(
                    keySelector: item => item.Key,
                    elementSelector: item => item.Value))
            .Build();
}