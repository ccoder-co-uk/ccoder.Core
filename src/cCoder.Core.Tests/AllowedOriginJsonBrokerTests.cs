// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Json;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests.Brokers.Json;

public sealed class AllowedOriginJsonBrokerTests
{
    [Fact]
    public void ExtractOrigins_WhenConfigurationContainsNestedOrigins_ReturnsOrigins()
    {
        // Given
        const string configJson = """
            {
                "cors": {
                    "allowedOrigins": [
                        "https://admin.example.com",
                        "https://api.example.com"
                    ]
                }
            }
            """;

        AllowedOriginJsonBroker broker = new();

        // When
        IEnumerable<string> actualOrigins = broker.ExtractOrigins(
            configJson: configJson);

        // Then
        actualOrigins.Should().BeEquivalentTo(
            expectation:
            [
                "https://admin.example.com",
                "https://api.example.com"
            ]);
    }

    [Fact]
    public void ExtractOrigins_WhenConfigurationIsInvalid_ReturnsNoOrigins()
    {
        // Given
        AllowedOriginJsonBroker broker = new();

        // When
        IEnumerable<string> actualOrigins = broker.ExtractOrigins(
            configJson: "{ invalid json }");

        // Then
        actualOrigins.Should().BeEmpty();
    }
}