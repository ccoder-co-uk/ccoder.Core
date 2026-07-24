// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Core.Services.Processings.AllowedOrigins;
using FluentAssertions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests.Cors;

public sealed class AllowedOriginStoreProcessingServiceTests
{
    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncPermitsLoopbackOrigins()
    {
        // given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: []);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // when
        bool localhostIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://localhost:3000");

        bool loopbackIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "http://127.0.0.1:5173");

        // then
        localhostIsAllowed.Should()
            .BeTrue();

        loopbackIsAllowed.Should()
            .BeTrue();
    }

    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncMatchesConfiguredHost()
    {
        // given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: ["app.example.com"]);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // when
        bool matchingOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://app.example.com");

        bool differentOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://other.example.com");

        // then
        matchingOriginIsAllowed.Should()
            .BeTrue();

        differentOriginIsAllowed.Should()
            .BeFalse();
    }

    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncMatchesConfiguredAuthority()
    {
        // given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: ["app.example.com:8443"]);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // when
        bool matchingAuthorityIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://app.example.com:8443");

        bool differentAuthorityIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://app.example.com:9443");

        // then
        matchingAuthorityIsAllowed.Should()
            .BeTrue();

        differentAuthorityIsAllowed.Should()
            .BeFalse();
    }

    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncRespectsConfiguredScheme()
    {
        // given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: ["https://secure.example.com"]);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // when
        bool secureOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://secure.example.com");

        bool insecureOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "http://secure.example.com");

        // then
        secureOriginIsAllowed.Should()
            .BeTrue();

        insecureOriginIsAllowed.Should()
            .BeFalse();
    }

    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncRejectsInvalidOrigins()
    {
        // given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: ["app.example.com"]);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // when
        bool malformedOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "not-an-origin");

        bool unsupportedOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "ftp://app.example.com");

        // then
        malformedOriginIsAllowed.Should()
            .BeFalse();

        unsupportedOriginIsAllowed.Should()
            .BeFalse();
    }
}