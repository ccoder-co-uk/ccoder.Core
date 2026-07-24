// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Services.Processings.AllowedOrigins;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests.Cors;

public sealed class AllowedOriginProcessingServiceTests
{
    private readonly AllowedOriginProcessingService service = new();

    [Fact]
    public void IsAllowed_ShouldPermitLoopbackOrigins()
    {
        CoreAllowedOriginSnapshot snapshot = service.CreateSnapshot(configuredOrigins: []);

        service.IsAllowed(origin: "https://localhost:3000",snapshot: snapshot)
            .Should()
            .BeTrue();

        service.IsAllowed(origin: "http://127.0.0.1:5173",snapshot: snapshot)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsAllowed_ShouldMatchConfiguredHostWithoutScheme()
    {
        CoreAllowedOriginSnapshot snapshot = service.CreateSnapshot(configuredOrigins: ["app.example.com"]);

        service.IsAllowed(origin: "https://app.example.com",snapshot: snapshot)
            .Should()
            .BeTrue();

        service.IsAllowed(origin: "http://app.example.com",snapshot: snapshot)
            .Should()
            .BeTrue();

        service.IsAllowed(origin: "https://other.example.com",snapshot: snapshot)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldMatchConfiguredAuthorityWithPort()
    {
        CoreAllowedOriginSnapshot snapshot = service.CreateSnapshot(configuredOrigins: ["app.example.com:8443"]);

        service.IsAllowed(origin: "https://app.example.com:8443",snapshot: snapshot)
            .Should()
            .BeTrue();

        service.IsAllowed(origin: "https://app.example.com",snapshot: snapshot)
            .Should()
            .BeFalse();

        service.IsAllowed(origin: "https://app.example.com:9443",snapshot: snapshot)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldRespectConfiguredOriginSchemeWhenProvided()
    {
        CoreAllowedOriginSnapshot snapshot =
            service.CreateSnapshot(configuredOrigins: ["https://secure.example.com"]);

        service.IsAllowed(origin: "https://secure.example.com",snapshot: snapshot)
            .Should()
            .BeTrue();

        service.IsAllowed(origin: "http://secure.example.com",snapshot: snapshot)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsAllowed_ShouldRejectInvalidOrigins()
    {
        CoreAllowedOriginSnapshot snapshot = service.CreateSnapshot(configuredOrigins: ["app.example.com"]);

        service.IsAllowed(origin: string.Empty,snapshot: snapshot)
            .Should()
            .BeFalse();

        service.IsAllowed(origin: "not-an-origin",snapshot: snapshot)
            .Should()
            .BeFalse();

        service.IsAllowed(origin: "ftp://app.example.com",snapshot: snapshot)
            .Should()
            .BeFalse();
    }
}