// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using cCoder.Security.Exposures;
using cCoder.Security.Models.Configurations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class ApiMetadataAuthorizationTests
{
    [Theory]
    [InlineData("/swagger")]
    [InlineData("/swagger/index.html")]
    [InlineData("/$odata")]
    [InlineData("/$ODATA/")]
    public void ShouldIdentifyGeneratedApiDocumentationPaths(
        string path)
    {
        // Given
        PathString requestPath = new(value: path);

        // When
        bool result =
            WebApplicationExtensions.IsApiDocumentationRequest(
                path: requestPath);

        // Then
        result.Should()
            .BeTrue();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Api")]
    [InlineData("/Api/B2B/$metadata")]
    [InlineData("/Documentation/Metadata")]
    public void ShouldNotIdentifyOrdinaryApplicationPathsAsApiDocumentation(
        string path)
    {
        // Given
        PathString requestPath = new(value: path);

        // When
        bool result =
            WebApplicationExtensions.IsApiDocumentationRequest(
                path: requestPath);

        // Then
        result.Should()
            .BeFalse();
    }

    [Fact]
    public void ShouldAuthorizeAuthenticatedMetadataReader()
    {
        // Given
        Mock<IApiMetadataAuthorizationManager>
            authorizationManagerMock =
            new();

        DefaultHttpContext context =
            CreateContext(
                userId: "metadata.reader",
                authorizationManager:
                    authorizationManagerMock.Object);

        // When
        bool result =
            WebApplicationExtensions
                .AuthorizeApiMetadataRequest(
                    context: context);

        // Then
        result.Should()
            .BeTrue();

        authorizationManagerMock.Verify(
            expression: manager => manager
                .EnsureUserCanReadApiMetadata(),
            times: Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Guest")]
    public void ShouldChallengeAnonymousMetadataReader(
        string userId)
    {
        // Given
        Mock<IApiMetadataAuthorizationManager>
            authorizationManagerMock =
            new();

        DefaultHttpContext context =
            CreateContext(
                userId: userId,
                authorizationManager:
                    authorizationManagerMock.Object);

        // When
        bool result =
            WebApplicationExtensions
                .AuthorizeApiMetadataRequest(
                    context: context);

        // Then
        result.Should()
            .BeFalse();

        context.Response.StatusCode
            .Should()
            .Be(expected: StatusCodes.Status401Unauthorized);

        context.Response.Headers.WWWAuthenticate
            .ToString()
            .Should()
            .Be(expected: "Bearer");

        authorizationManagerMock.Verify(
            expression: manager => manager
                .EnsureUserCanReadApiMetadata(),
            times: Times.Never);
    }

    [Fact]
    public void ShouldForbidMetadataReaderWithoutPrivilege()
    {
        // Given
        Mock<IApiMetadataAuthorizationManager>
            authorizationManagerMock =
            new();

        authorizationManagerMock.Setup(
            expression: manager => manager
                .EnsureUserCanReadApiMetadata())
            .Throws(exception: new SecurityException());

        DefaultHttpContext context =
            CreateContext(
                userId: "ordinary.user",
                authorizationManager:
                    authorizationManagerMock.Object);

        // When
        bool result =
            WebApplicationExtensions
                .AuthorizeApiMetadataRequest(
                    context: context);

        // Then
        result.Should()
            .BeFalse();

        context.Response.StatusCode
            .Should()
            .Be(expected: StatusCodes.Status403Forbidden);
    }

    private static DefaultHttpContext CreateContext(
        string userId,
        IApiMetadataAuthorizationManager
            authorizationManager)
    {
        ServiceCollection services = new();
        Mock<ISSOAuthInfo> authInfoMock = new();

        authInfoMock.SetupGet(
            expression: authInfo => authInfo.SSOUserId)
            .Returns(value: userId);

        services.AddSingleton(
            implementationInstance: authInfoMock.Object);

        services.AddSingleton(
            implementationInstance: authorizationManager);

        return new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };
    }
}