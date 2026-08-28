// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace Web.AcceptanceTests.Tests;

public sealed partial class AppConfigurationTests
{
    [Fact]
    public void WebHost_ShouldExposeStandardRootConfigurationType()
    {
        // Given
        Type hostMarker = typeof(Web.Program);

        // When
        Type appConfigurationType = hostMarker.Assembly
            .GetType(name: "Web.Models.AppConfiguration");

        // Then
        appConfigurationType
            .Should()
            .NotBeNull();
    }
}