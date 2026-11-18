// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;

using cCoder.Logging.Brokers.Loggings;
using FluentAssertions;
using HostedServices.Controllers;
using HostedServices.Exposures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HostedServices.AcceptanceTests.Tests.Api;

public sealed partial class WorkflowControllerFailureTests
{
    [Fact]
    public async Task Post_WhenRequestIsInvalid_ShouldWarnAndReturnBadRequest()
    {
        // Given
        TestLoggingBroker loggingBroker = new();

        WorkflowController controller = CreateController(
            exception: new ArgumentException(message: "invalid"),
            loggingBroker: loggingBroker);

        // When
        IActionResult result = await controller.Post(flowId: Guid.NewGuid());

        // Then
        result
            .Should()
            .BeOfType<BadRequestObjectResult>();

        loggingBroker.WarningCount
            .Should()
            .Be(expected: 1);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: 0);
    }

    [Fact]
    public async Task Post_WhenSecurityRejectsRequest_ShouldWarnAndReturnForbidden()
    {
        // Given
        TestLoggingBroker loggingBroker = new();

        WorkflowController controller = CreateController(
            exception: new System.Security.SecurityException(message: "denied"),
            loggingBroker: loggingBroker);

        // When
        IActionResult result = await controller.Post(flowId: Guid.NewGuid());

        // Then
        StatusCodeResult response = result
            .Should()
            .BeOfType<StatusCodeResult>()
            .Subject;

        response.StatusCode
            .Should()
            .Be(expected: StatusCodes.Status403Forbidden);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: 1);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: 0);
    }

    [Fact]
    public async Task Post_WhenRequestFails_ShouldLogErrorAndReturnServerError()
    {
        // Given
        TestLoggingBroker loggingBroker = new();

        WorkflowController controller = CreateController(
            exception: new InvalidOperationException(message: "failed"),
            loggingBroker: loggingBroker);

        // When
        IActionResult result = await controller.Post(flowId: Guid.NewGuid());

        // Then
        StatusCodeResult response = result
            .Should()
            .BeOfType<StatusCodeResult>()
            .Subject;

        response.StatusCode
            .Should()
            .Be(expected: StatusCodes.Status500InternalServerError);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: 0);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: 1);
    }

    private static WorkflowController CreateController(
        Exception exception,
        TestLoggingBroker loggingBroker) =>
        new(
            workflowInstanceProcessingService:
                new FailingWorkflowInstanceManager(exception: exception),
            loggingBroker: loggingBroker);

    private sealed class FailingWorkflowInstanceManager(Exception exception)
        : IHostedWorkflowInstanceManager
    {
        public ValueTask ExecuteWaitingQueuedInstanceByIdAsync(
            Guid flowInstanceDataId) =>
            ValueTask.FromException(exception: exception);

        public object[] GetStats() =>
            throw exception;
    }

    private sealed class TestLoggingBroker : ILoggingBroker
    {
        public int WarningCount { get; private set; }

        public int ErrorCount { get; private set; }

        public void LogDebug(string message, params object[] args)
        { }

        public void LogInformation(string message, params object[] args)
        { }

        public void LogWarning(string message, params object[] args) =>
            WarningCount++;

        public void LogError(string message, params object[] args) =>
            ErrorCount++;

        public void LogError(
            Exception exception,
            string message,
            params object[] args) =>
            ErrorCount++;
    }
}