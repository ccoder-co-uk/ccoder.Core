// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed class ApiExposureTests
{
    [Theory]
    [InlineData(null, false, true)]
    [InlineData(null, true, false)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    public void ShouldExposeApiSurfaceEvaluatesConfigurationAndEnvironment(
        bool? configuredValue,
        bool isProduction,
        bool expected)
    {
        // When
        bool result =
            WebApplicationExtensions.ShouldExposeApiSurface(
                configuredValue: configuredValue,
                isProduction: isProduction);

        // Then
        result.Should()
            .Be(expected: expected);
    }
}
