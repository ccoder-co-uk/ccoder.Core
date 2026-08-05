// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class ContentSecurityPolicyTests
{
    [Fact]
    public void CreateContentSecurityPolicy_ShouldUseNonceWithoutUnsafeSources()
    {
        // Given
        const string nonce = "test-nonce";

        // When
        string policy =
            WebApplicationExtensions.CreateContentSecurityPolicy(
                allowEditorFraming: false,
                nonce: nonce);

        // Then
        policy.Should()
            .Contain(expected: $"script-src 'self' 'nonce-{nonce}'");

        policy.Should()
            .Contain(expected: $"style-src 'self' 'nonce-{nonce}'");

        policy.Should()
            .Contain(expected: "script-src-attr 'none'");

        policy.Should()
            .Contain(expected: "style-src-attr 'none'");

        policy.Should()
            .Contain(expected: "frame-ancestors 'none'");

        policy.Should()
            .NotContain(unexpected: "unsafe-inline");

        policy.Should()
            .NotContain(unexpected: "unsafe-eval");

        policy.Should()
            .NotContain(unexpected: "https:");

        policy.Should()
            .NotContain(unexpected: "wss:");
    }

    [Fact]
    public void CreateContentSecurityPolicy_ShouldAllowEditorFramingWithoutFrameAncestorRestriction()
    {
        // Given

        // When
        string policy =
            WebApplicationExtensions.CreateContentSecurityPolicy(
                allowEditorFraming: true,
                nonce: "test-nonce");

        // Then
        policy.Should()
            .NotContain(unexpected: "frame-ancestors");
    }
}