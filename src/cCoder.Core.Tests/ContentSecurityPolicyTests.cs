// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using cCoder.Core.Models;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class ContentSecurityPolicyTests
{
    [Fact]
    public void CreateContentSecurityPolicy_ShouldUseNonceWithoutUnsafeScriptSources()
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
            .Contain(expected: "style-src-attr 'unsafe-inline'");

        policy.Should()
            .Contain(expected: "style-src-elem 'self' 'unsafe-inline'");

        policy.Should()
            .Contain(expected: "worker-src 'self' blob:");

        policy.Should()
            .Contain(expected: "frame-ancestors 'none'");

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

    [Fact]
    public void CreateContentSecurityPolicy_ShouldAppendConfiguredSourcesToTheirDirectives()
    {
        // Given
        ContentSecurityPolicyConfiguration configuration = new()
        {
            ConnectSources = ["wss://stream.example.test"],
            FontSources = ["https://cdn.example.test"],
            StyleSources = ["https://cdn.example.test"]
        };

        // When
        string policy =
            WebApplicationExtensions.CreateContentSecurityPolicy(
                allowEditorFraming: false,
                nonce: "test-nonce",
                configuration: configuration);

        // Then
        policy.Should()
            .Contain(expected: "style-src-elem 'self' 'unsafe-inline' https://cdn.example.test");

        policy.Should()
            .Contain(expected: "font-src 'self' data: https://cdn.example.test");

        policy.Should()
            .Contain(expected: "connect-src 'self' wss://stream.example.test");
    }

    [Fact]
    public void CreateContentSecurityPolicy_ShouldIgnoreConfiguredDirectiveInjection()
    {
        // Given
        ContentSecurityPolicyConfiguration configuration = new()
        {
            ScriptSources =
            [
                "https://valid.example.test",
                "https://bad.example.test; object-src *",
                "javascript:alert(1)"
            ]
        };

        // When
        string policy =
            WebApplicationExtensions.CreateContentSecurityPolicy(
                allowEditorFraming: false,
                nonce: "test-nonce",
                configuration: configuration);

        // Then
        policy.Should()
            .Contain(expected: "script-src 'self' 'nonce-test-nonce' https://valid.example.test");

        policy.Should()
            .NotContain(unexpected: "bad.example.test");

        policy.Should()
            .NotContain(unexpected: "javascript:");
    }
}