// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class FirstTimeSetupStateOrchestrationServiceTests
{
    [Fact]
    public async Task IsInitializedAsyncShouldReturnTrueForInitializedStores()
    {
        // Given
        coreSetupStateServiceMock
            .Setup(expression: service =>
                service.IsCoreInitializedAsync(
                    cancellationToken:
                        It.IsAny<CancellationToken>()))
            .Returns(value: ValueTask.FromResult(result: true));

        securitySetupStateServiceMock
            .Setup(expression: service =>
                service.IsSecurityInitializedAsync(
                    cancellationToken:
                        It.IsAny<CancellationToken>()))
            .Returns(value: ValueTask.FromResult(result: true));

        // When
        bool isInitialized =
            await firstTimeSetupStateOrchestrationService
                .IsInitializedAsync();

        // Then
        isInitialized.Should()
            .BeTrue();
    }

    [Fact]
    public async Task IsInitializedAsyncShouldStopForUninitializedCoreStore()
    {
        // Given
        coreSetupStateServiceMock
            .Setup(expression: service =>
                service.IsCoreInitializedAsync(
                    cancellationToken:
                        It.IsAny<CancellationToken>()))
            .Returns(value: ValueTask.FromResult(result: false));

        // When
        bool isInitialized =
            await firstTimeSetupStateOrchestrationService
                .IsInitializedAsync();

        // Then
        isInitialized.Should()
            .BeFalse();

        securitySetupStateServiceMock.Verify(
            expression: service =>
                service.IsSecurityInitializedAsync(
                    cancellationToken:
                        It.IsAny<CancellationToken>()),
            times: Times.Never);
    }

    [Fact]
    public async Task IsInitializedAsyncShouldWrapValidationException()
    {
        // Given
        CoreValidationException validationException = new(
            innerException: new ArgumentException());

        coreSetupStateServiceMock
            .Setup(expression: service =>
                service.IsCoreInitializedAsync(
                    cancellationToken:
                        It.IsAny<CancellationToken>()))
            .Throws(exception: validationException);

        // When
        Func<Task> initializeAction = async () =>
            await firstTimeSetupStateOrchestrationService
                .IsInitializedAsync();

        // Then
        CoreOrchestrationValidationException actualException =
            (await initializeAction.Should()
                .ThrowExactlyAsync<
                    CoreOrchestrationValidationException>())
                .Which;

        actualException.InnerException.Should()
            .BeOfType<CoreValidationException>();
    }

    [Fact]
    public async Task IsInitializedAsyncShouldWrapUnexpectedException()
    {
        // Given
        Exception unexpectedException = new();

        coreSetupStateServiceMock
            .Setup(expression: service =>
                service.IsCoreInitializedAsync(
                    cancellationToken:
                        It.IsAny<CancellationToken>()))
            .Throws(exception: unexpectedException);

        // When
        Func<Task> initializeAction = async () =>
            await firstTimeSetupStateOrchestrationService
                .IsInitializedAsync();

        // Then
        CoreOrchestrationServiceException actualException =
            (await initializeAction.Should()
                .ThrowExactlyAsync<
                    CoreOrchestrationServiceException>())
                .Which;

        actualException.InnerException.Should()
            .BeOfType<Exception>();
    }

    [Fact]
    public async Task IsInitializedAsyncShouldWrapDependencyException()
    {
        // Given
        CoreDependencyException dependencyException = new(
            innerException: new Exception());

        coreSetupStateServiceMock
            .Setup(expression: service =>
                service.IsCoreInitializedAsync(
                    cancellationToken:
                        It.IsAny<CancellationToken>()))
            .Throws(exception: dependencyException);

        // When
        Func<Task> initializeAction = async () =>
            await firstTimeSetupStateOrchestrationService
                .IsInitializedAsync();

        // Then
        CoreOrchestrationDependencyException actualException =
            (await initializeAction.Should()
                .ThrowExactlyAsync<
                    CoreOrchestrationDependencyException>())
                .Which;

        actualException.InnerException.Should()
            .BeOfType<CoreDependencyException>();
    }
}