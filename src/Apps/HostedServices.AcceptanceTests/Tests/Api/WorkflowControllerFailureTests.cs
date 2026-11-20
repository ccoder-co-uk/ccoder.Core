// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Security;
using System.Security.Authentication;
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
    public async Task Actions_WhenDependenciesSucceed_ShouldReturnSuccessAndLogInformation()
    {
        // Given
        TestLoggingBroker loggingBroker = new();

        WorkflowController controller = CreateController(
            manager: new SuccessfulWorkflowInstanceManager(),
            loggingBroker: loggingBroker);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // When
        IActionResult getResult = controller.Get();
        IActionResult postResult = await controller.Post(flowId: Guid.NewGuid());
        IActionResult statsResult = controller.GetStats();

        // Then
        getResult
            .Should()
            .BeOfType<ViewResult>();

        postResult
            .Should()
            .BeOfType<OkResult>();

        statsResult
            .Should()
            .BeOfType<JsonResult>();

        loggingBroker.InformationCount
            .Should()
            .Be(expected: 3);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: 0);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: 0);
    }

    [Fact]
    public async Task Post_WhenAuthenticationFails_ShouldWarnAndReturnUnauthorized()
    {
        // Given
        const string safeReason = "authentication expired";
        TestLoggingBroker loggingBroker = new();

        WorkflowController controller = CreateController(
            exception: new AuthenticationException(message: safeReason),
            loggingBroker: loggingBroker);

        // When
        IActionResult result = await controller.Post(flowId: Guid.NewGuid());

        // Then
        UnauthorizedObjectResult response = result
            .Should()
            .BeOfType<UnauthorizedObjectResult>()
            .Subject;

        response.Value
            .Should()
            .Be(expected: "Authentication failed.");

        loggingBroker.InformationCount
            .Should()
            .Be(expected: 0);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: 1);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: 0);
    }

    [Theory]
    [InlineData("authentication", StatusCodes.Status401Unauthorized, 1, 0)]
    [InlineData("security", StatusCodes.Status403Forbidden, 1, 0)]
    [InlineData("unexpected", StatusCodes.Status500InternalServerError, 0, 1)]
    public void Get_WhenViewLoadingFails_ShouldReturnClassifiedResponse(
        string failure,
        int expectedStatus,
        int expectedWarningCount,
        int expectedErrorCount)
    {
        // Given
        Exception exception = CreateException(failure: failure);
        TestLoggingBroker loggingBroker = new(informationException: exception);

        WorkflowController controller = CreateController(
            manager: new SuccessfulWorkflowInstanceManager(),
            loggingBroker: loggingBroker);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // When
        IActionResult result = controller.Get();

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: expectedStatus);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: expectedWarningCount);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: expectedErrorCount);
    }

    [Theory]
    [InlineData("authentication", StatusCodes.Status401Unauthorized, 1, 0)]
    [InlineData("security", StatusCodes.Status403Forbidden, 1, 0)]
    [InlineData("unexpected", StatusCodes.Status500InternalServerError, 0, 1)]
    public void GetStats_WhenDependencyFails_ShouldReturnClassifiedResponse(
        string failure,
        int expectedStatus,
        int expectedWarningCount,
        int expectedErrorCount)
    {
        // Given
        Exception exception = CreateException(failure: failure);
        TestLoggingBroker loggingBroker = new();

        WorkflowController controller = CreateController(
            exception: exception,
            loggingBroker: loggingBroker);

        // When
        IActionResult result = controller.GetStats();

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: expectedStatus);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: expectedWarningCount);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: expectedErrorCount);
    }

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
        CreateController(
            manager: new FailingWorkflowInstanceManager(exception: exception),
            loggingBroker: loggingBroker);

    private static WorkflowController CreateController(
        IHostedWorkflowInstanceManager manager,
        TestLoggingBroker loggingBroker) =>
        new(
            workflowInstanceProcessingService: manager,
            loggingBroker: loggingBroker);

    private static Exception CreateException(string failure) =>
        failure switch
        {
            "authentication" => new AuthenticationException(message: "authentication expired"),
            "security" => new SecurityException(message: "access denied"),
            _ => new InvalidOperationException(message: "internal detail")
        };

    private static int GetStatusCode(IActionResult result) =>
        result switch
        {
            ObjectResult objectResult => objectResult.StatusCode ?? StatusCodes.Status200OK,
            StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
            _ => StatusCodes.Status200OK
        };

    private sealed class SuccessfulWorkflowInstanceManager
        : IHostedWorkflowInstanceManager
    {
        public ValueTask ExecuteWaitingQueuedInstanceByIdAsync(
            Guid flowInstanceDataId) =>
            ValueTask.CompletedTask;

        public object[] GetStats() =>
            [new object()];
    }

    private sealed class FailingWorkflowInstanceManager(Exception exception)
        : IHostedWorkflowInstanceManager
    {
        public ValueTask ExecuteWaitingQueuedInstanceByIdAsync(
            Guid flowInstanceDataId) =>
            ValueTask.FromException(exception: exception);

        public object[] GetStats() =>
            throw exception;
    }

    private sealed class TestLoggingBroker(Exception informationException = null)
        : ILoggingBroker
    {
        public int InformationCount { get; private set; }

        public int WarningCount { get; private set; }

        public int ErrorCount { get; private set; }

        public void LogDebug(string message, params object[] args)
        { }

        public void LogInformation(string message, params object[] args)
        {
            InformationCount++;

            if (informationException is not null)
            {
                throw informationException;
            }
        }

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