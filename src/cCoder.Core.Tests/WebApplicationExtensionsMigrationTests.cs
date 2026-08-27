// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class WebApplicationExtensionsMigrationTests
{
    [Fact]
    public void ResolveMigrationConnectionString_ShouldUseConfiguredAdminConnection()
    {
        // Given
        const string regularConnection = "Server=regular;Database=Runtime;";
        const string adminConnection = "Server=admin;Database=Migrations;";

        // When
        string result =
            WebApplicationExtensions.ResolveMigrationConnectionString(
                adminConnectionString: adminConnection,
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

        // When
        string result =
            WebApplicationExtensions.ResolveMigrationConnectionString(
                adminConnectionString: adminConnection,
                regularConnectionString: regularConnection);

        // Then
        result.Should()
            .Be(expected: regularConnection);
    }

}