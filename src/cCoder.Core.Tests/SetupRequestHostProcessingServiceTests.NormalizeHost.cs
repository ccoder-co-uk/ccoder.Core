// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class SetupRequestHostProcessingServiceTests
{
    [Fact]
    public void NormalizeHostShouldNormalizeHost()
    {
        // Given
        const string host = " WWW.Example.COM ";

        // When
        string actualHost =
            setupRequestHostProcessingService.NormalizeHost(
                host: host);

        // Then
        actualHost.Should()
            .Be(expected: "example.com");
    }

    [Fact]
    public void NormalizeHostShouldThrowValidationExceptionForMissingHost()
    {
        // Given
        const string invalidHost = null;

        // When
        Action normalizeHostAction = () =>
            setupRequestHostProcessingService.NormalizeHost(
                host: invalidHost);

        // Then
        normalizeHostAction.Should()
            .ThrowExactly<CoreProcessingValidationException>()
            .WithInnerException<ArgumentException>();
    }
}
