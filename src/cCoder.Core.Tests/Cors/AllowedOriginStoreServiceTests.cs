// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.AllowedOrigins;
using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Data.Models.CMS;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace cCoder.Core.Tests.Cors;

public sealed partial class AllowedOriginStoreServiceTests
{
    [Fact]
    public async Task GetAllowedOriginsAsyncReturnsCurrentAppOrigins()
    {
        // Given
        Mock<IAllowedOriginStoreBroker> allowedOriginStoreBrokerMock = new();

        allowedOriginStoreBrokerMock
            .Setup(expression: broker => broker.GetAllowedOrigins())
            .Returns(
                value:
                [
                    "app.example.com",
                    "https://admin.example.com"
                ]);

        AllowedOriginStoreService service = new(
            allowedOriginStoreBroker: allowedOriginStoreBrokerMock.Object);

        // When
        string[] actualOrigins = await service.GetAllowedOriginsAsync();

        // Then
        actualOrigins.Should()
            .BeEquivalentTo(
                expectation:
                [
                    "app.example.com",
                    "https://admin.example.com"
                ]);
    }

    [Fact]
    public async Task GetAllowedOriginsAsyncReturnsEmptyWithoutRequest()
    {
        // Given
        Mock<IAllowedOriginStoreBroker> allowedOriginStoreBrokerMock = new();

        allowedOriginStoreBrokerMock
            .Setup(expression: broker => broker.GetAllowedOrigins())
            .Returns(value: []);

        AllowedOriginStoreService service = new(
            allowedOriginStoreBroker: allowedOriginStoreBrokerMock.Object);

        // When
        string[] actualOrigins = await service.GetAllowedOriginsAsync();

        // Then
        actualOrigins.Should()
            .BeEmpty();

        allowedOriginStoreBrokerMock.Verify(
            expression: broker => broker.GetAllowedOrigins(),
            times: Times.Once);
    }

    [Fact]
    public async Task GetAllowedOriginsAsyncReturnsEmptyWithoutCurrentApp()
    {
        // Given
        Mock<IAllowedOriginStoreBroker> allowedOriginStoreBrokerMock = new();

        allowedOriginStoreBrokerMock
            .Setup(expression: broker => broker.GetAllowedOrigins())
            .Returns(value: []);

        AllowedOriginStoreService service = new(
            allowedOriginStoreBroker: allowedOriginStoreBrokerMock.Object);

        // When
        string[] actualOrigins = await service.GetAllowedOriginsAsync();

        // Then
        actualOrigins.Should()
            .BeEmpty();
    }

}