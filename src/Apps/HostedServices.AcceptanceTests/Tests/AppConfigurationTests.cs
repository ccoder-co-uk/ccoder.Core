// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace HostedServices.AcceptanceTests.Tests;

public sealed partial class AppConfigurationTests
{
    [Fact]
    public void HostedServicesHost_ShouldExposeStandardRootConfigurationType()
    {
        // Given
        Type hostMarker = typeof(HostedServices.Program);

        // When
        Type appConfigurationType = hostMarker.Assembly
            .GetType(name: "HostedServices.Models.AppConfiguration");

        // Then
        appConfigurationType
            .Should()
            .NotBeNull();
    }
}