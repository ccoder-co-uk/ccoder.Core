// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Core.Services.Processings.AllowedOrigins;
using FluentAssertions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests.Cors;

public sealed partial class AllowedOriginStoreProcessingServiceTests
{
    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncPermitsLoopbackOrigins()
    {
        // Given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: []);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // When
        bool localhostIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://localhost:3000");

        bool loopbackIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "http://127.0.0.1:5173");

        // Then
        localhostIsAllowed.Should()
            .BeTrue();

        loopbackIsAllowed.Should()
            .BeTrue();
    }

    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncMatchesConfiguredHost()
    {
        // Given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: ["app.example.com"]);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // When
        bool matchingOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://app.example.com");

        bool differentOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://other.example.com");

        // Then
        matchingOriginIsAllowed.Should()
            .BeTrue();

        differentOriginIsAllowed.Should()
            .BeFalse();
    }

    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncMatchesConfiguredAuthority()
    {
        // Given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: ["app.example.com:8443"]);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // When
        bool matchingAuthorityIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://app.example.com:8443");

        bool differentAuthorityIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://app.example.com:9443");

        // Then
        matchingAuthorityIsAllowed.Should()
            .BeTrue();

        differentAuthorityIsAllowed.Should()
            .BeFalse();
    }

    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncRespectsConfiguredScheme()
    {
        // Given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: ["https://secure.example.com"]);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // When
        bool secureOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "https://secure.example.com");

        bool insecureOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "http://secure.example.com");

        // Then
        secureOriginIsAllowed.Should()
            .BeTrue();

        insecureOriginIsAllowed.Should()
            .BeFalse();
    }

    [Fact]
    public async Task IsCoreAllowedOriginAllowedAsyncRejectsInvalidOrigins()
    {
        // Given
        Mock<IAllowedOriginStoreService> allowedOriginStoreServiceMock = new();

        allowedOriginStoreServiceMock
            .Setup(expression: service => service.GetAllowedOriginsAsync())
            .ReturnsAsync(value: ["app.example.com"]);

        AllowedOriginStoreProcessingService service = new(
            allowedOriginStoreService: allowedOriginStoreServiceMock.Object);

        // When
        bool malformedOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "not-an-origin");

        bool unsupportedOriginIsAllowed =
            await service.IsCoreAllowedOriginAllowedAsync(
                origin: "ftp://app.example.com");

        // Then
        malformedOriginIsAllowed.Should()
            .BeFalse();

        unsupportedOriginIsAllowed.Should()
            .BeFalse();
    }
}