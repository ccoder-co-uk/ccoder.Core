// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class WebApplicationExtensionsMigrationTests
{
    [Theory]
    [InlineData("Core", "CoreAdmin")]
    [InlineData("Security", "SecurityAdmin")]
    public void ResolveMigrationConnectionString_ShouldUseConfiguredAdminConnection(
        string databaseName,
        string adminConnectionName)
    {
        // Given
        const string regularConnection = "Server=regular;Database=Runtime;";
        const string adminConnection = "Server=admin;Database=Migrations;";

        IConfiguration configuration =
            CreateConfiguration(
                connectionStrings:
                [
                    (adminConnectionName, adminConnection)
                ]);

        // When
        string result =
            WebApplicationExtensions.ResolveMigrationConnectionString(
                configuration: configuration,
                databaseName: databaseName,
                regularConnectionString: regularConnection);

        // Then
        result.Should()
            .Be(expected: adminConnection);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveMigrationConnectionString_ShouldUseRegularConnectionWhenAdminIsUnavailable(
        string adminConnection)
    {
        // Given
        const string regularConnection = "Server=regular;Database=Core;";

        IConfiguration configuration =
            CreateConfiguration(
                connectionStrings:
                [
                    ("CoreAdmin", adminConnection)
                ]);

        // When
        string result =
            WebApplicationExtensions.ResolveMigrationConnectionString(
                configuration: configuration,
                databaseName: "Core",
                regularConnectionString: regularConnection);

        // Then
        result.Should()
            .Be(expected: regularConnection);
    }

    private static IConfiguration CreateConfiguration(
        params (string Name, string Value)[] connectionStrings)
    {
        Dictionary<string, string> values = connectionStrings
            .Where(predicate: item => item.Value is not null)
            .ToDictionary(
                keySelector: item => $"ConnectionStrings:{item.Name}",
                elementSelector: item => item.Value);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: values)
            .Build();
    }
}