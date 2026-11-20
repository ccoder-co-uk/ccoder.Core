// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Linq;
using System.Security;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Exposures.Controllers;
using cCoder.Core.Models;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Mail.Models;
using cCoder.Packaging.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class CoreOwnedControllerContractTests
{
    [Fact]
    public async Task TemplatedEmailPost_WhenDependencyCompletes_ShouldReturnOkAndLogInformation()
    {
        // Given
        QueuedEmail queuedEmail = new();
        Mock<ITemplatedEmailOrchestrationService> service = new();
        Mock<ILoggingBroker> loggingBroker = new();

        service
            .Setup(expression: service => service.QueueTemplatedEmailDetailsAsync(
                templatedEmailDetails: It.IsAny<TemplatedEmailDetails>()))
            .Returns(value: ValueTask.FromResult(result: queuedEmail));

        TemplatedEmailController controller = new(
            templatedEmailOrchestrationService: service.Object,
            loggingBroker: loggingBroker.Object);

        // When
        IActionResult result = await controller.Post(
            newTemplatedEmailDetails: new TemplatedEmailDetails());

        // Then
        result
            .Should()
            .BeOfType<OkObjectResult>();

        VerifyLogCounts(loggingBroker: loggingBroker, information: 1);
    }

    [Theory]
    [InlineData(true, StatusCodes.Status401Unauthorized)]
    [InlineData(false, StatusCodes.Status500InternalServerError)]
    public async Task TemplatedEmailPost_WhenDependencyFails_ShouldReturnClassifiedResponse(
        bool authenticationFailure,
        int expectedStatus)
    {
        // Given
        Exception exception = authenticationFailure
            ? new AuthenticationException(message: "authentication expired")
            : new InvalidOperationException(message: "database detail");

        Mock<ITemplatedEmailOrchestrationService> service = new();
        Mock<ILoggingBroker> loggingBroker = new();

        service
            .Setup(expression: service => service.QueueTemplatedEmailDetailsAsync(
                templatedEmailDetails: It.IsAny<TemplatedEmailDetails>()))
            .Returns(value: ValueTask.FromException<QueuedEmail>(exception: exception));

        TemplatedEmailController controller = new(
            templatedEmailOrchestrationService: service.Object,
            loggingBroker: loggingBroker.Object);

        // When
        IActionResult result = await controller.Post(
            newTemplatedEmailDetails: new TemplatedEmailDetails());

        // Then
        ObjectResult response = result
            .Should()
            .BeAssignableTo<ObjectResult>()
            .Subject;

        response.StatusCode
            .Should()
            .Be(expected: expectedStatus);

        response.Value
            .Should()
            .NotBe(unexpected: "database detail");

        VerifyLogCounts(
            loggingBroker: loggingBroker,
            warning: authenticationFailure ? 1 : 0,
            error: authenticationFailure ? 0 : 1);
    }

    [Fact]
    public async Task TemplatedEmailPost_WhenAuthorizationFails_ShouldReturnForbiddenAndWarn()
    {
        // Given
        Mock<ITemplatedEmailOrchestrationService> service = new();
        Mock<ILoggingBroker> loggingBroker = new();

        service
            .Setup(expression: service => service.QueueTemplatedEmailDetailsAsync(
                templatedEmailDetails: It.IsAny<TemplatedEmailDetails>()))
            .Returns(value: ValueTask.FromException<QueuedEmail>(
                exception: new SecurityException(message: "access denied")));

        TemplatedEmailController controller = new(
            templatedEmailOrchestrationService: service.Object,
            loggingBroker: loggingBroker.Object);

        // When
        IActionResult result = await controller.Post(
            newTemplatedEmailDetails: new TemplatedEmailDetails());

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: StatusCodes.Status403Forbidden);

        VerifyLogCounts(loggingBroker: loggingBroker, warning: 1);
    }

    [Fact]
    public async Task TemplatedEmailPost_WhenModelStateIsInvalid_ShouldReturnBadRequestAndWarn()
    {
        // Given
        Mock<ITemplatedEmailOrchestrationService> service = new();
        Mock<ILoggingBroker> loggingBroker = new();

        TemplatedEmailController controller = new(
            templatedEmailOrchestrationService: service.Object,
            loggingBroker: loggingBroker.Object);

        controller.ModelState.AddModelError(key: "request", errorMessage: "invalid");

        // When
        IActionResult result = await controller.Post(
            newTemplatedEmailDetails: new TemplatedEmailDetails());

        // Then
        result
            .Should()
            .BeOfType<BadRequestObjectResult>();

        VerifyLogCounts(loggingBroker: loggingBroker, warning: 1);
    }

    [Theory]
    [InlineData("Post", StatusCodes.Status201Created)]
    [InlineData("Put", StatusCodes.Status200OK)]
    [InlineData("PutAggregateRoute", StatusCodes.Status200OK)]
    [InlineData("Delete", StatusCodes.Status202Accepted)]
    [InlineData("DeleteAggregateRoute", StatusCodes.Status202Accepted)]
    public async Task AppActions_WhenDependencyCompletes_ShouldReturnSuccessAndLogInformation(
        string action,
        int expectedStatus)
    {
        // Given
        App app = new() { Id = 7 };
        Mock<IAppAggregationService> service = CreateSuccessfulAppService(app: app);
        Mock<ILoggingBroker> loggingBroker = new();

        AppController controller = new(
            service: service.Object,
            loggingBroker: loggingBroker.Object);

        // When
        IActionResult result = action switch
        {
            "Post" => await controller.Post(newApp: app),
            "Put" => await controller.Put(key: app.Id, updatedApp: app),
            "PutAggregateRoute" => await controller.PutAggregateRoute(key: app.Id, updatedApp: app),
            "Delete" => await controller.Delete(key: app.Id),
            _ => await controller.DeleteAggregateRoute(key: app.Id)
        };

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: expectedStatus);

        VerifyLogCounts(loggingBroker: loggingBroker, information: 1);
    }

    [Theory]
    [InlineData("Post", true, StatusCodes.Status401Unauthorized)]
    [InlineData("Put", true, StatusCodes.Status401Unauthorized)]
    [InlineData("PutAggregateRoute", true, StatusCodes.Status401Unauthorized)]
    [InlineData("Delete", true, StatusCodes.Status401Unauthorized)]
    [InlineData("DeleteAggregateRoute", true, StatusCodes.Status401Unauthorized)]
    [InlineData("Post", false, StatusCodes.Status500InternalServerError)]
    [InlineData("Put", false, StatusCodes.Status500InternalServerError)]
    [InlineData("PutAggregateRoute", false, StatusCodes.Status500InternalServerError)]
    [InlineData("Delete", false, StatusCodes.Status500InternalServerError)]
    [InlineData("DeleteAggregateRoute", false, StatusCodes.Status500InternalServerError)]
    public async Task AppActions_WhenDependencyFails_ShouldReturnClassifiedResponse(
        string action,
        bool authenticationFailure,
        int expectedStatus)
    {
        // Given
        Exception exception = authenticationFailure
            ? new AuthenticationException(message: "authentication expired")
            : new InvalidOperationException(message: "database detail");

        App app = new() { Id = 7 };
        Mock<IAppAggregationService> service = CreateFailingAppService(exception: exception);
        Mock<ILoggingBroker> loggingBroker = new();

        AppController controller = new(
            service: service.Object,
            loggingBroker: loggingBroker.Object);

        // When
        IActionResult result = action switch
        {
            "Post" => await controller.Post(newApp: app),
            "Put" => await controller.Put(key: app.Id, updatedApp: app),
            "PutAggregateRoute" => await controller.PutAggregateRoute(key: app.Id, updatedApp: app),
            "Delete" => await controller.Delete(key: app.Id),
            _ => await controller.DeleteAggregateRoute(key: app.Id)
        };

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: expectedStatus);

        VerifyLogCounts(
            loggingBroker: loggingBroker,
            warning: authenticationFailure ? 1 : 0,
            error: authenticationFailure ? 0 : 1);
    }

    [Theory]
    [InlineData("Post")]
    [InlineData("Put")]
    [InlineData("PutAggregateRoute")]
    [InlineData("Delete")]
    [InlineData("DeleteAggregateRoute")]
    public async Task AppActions_WhenAuthorizationFails_ShouldReturnForbiddenAndWarn(string action)
    {
        // Given
        App app = new() { Id = 7 };

        Mock<IAppAggregationService> service = CreateFailingAppService(
            exception: new SecurityException(message: "access denied"));

        Mock<ILoggingBroker> loggingBroker = new();

        AppController controller = new(
            service: service.Object,
            loggingBroker: loggingBroker.Object);

        // When
        IActionResult result = action switch
        {
            "Post" => await controller.Post(newApp: app),
            "Put" => await controller.Put(key: app.Id, updatedApp: app),
            "PutAggregateRoute" => await controller.PutAggregateRoute(key: app.Id, updatedApp: app),
            "Delete" => await controller.Delete(key: app.Id),
            _ => await controller.DeleteAggregateRoute(key: app.Id)
        };

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: StatusCodes.Status403Forbidden);

        VerifyLogCounts(loggingBroker: loggingBroker, warning: 1);
    }

    [Theory]
    [InlineData("Post")]
    [InlineData("Put")]
    [InlineData("PutAggregateRoute")]
    public async Task AppWriteActions_WhenModelStateIsInvalid_ShouldReturnBadRequestAndWarn(
        string action)
    {
        // Given
        App app = new() { Id = 7 };
        Mock<IAppAggregationService> service = new();
        Mock<ILoggingBroker> loggingBroker = new();

        AppController controller = new(
            service: service.Object,
            loggingBroker: loggingBroker.Object);

        controller.ModelState.AddModelError(key: "request", errorMessage: "invalid");

        // When
        IActionResult result = action switch
        {
            "Post" => await controller.Post(newApp: app),
            "Put" => await controller.Put(key: app.Id, updatedApp: app),
            _ => await controller.PutAggregateRoute(key: app.Id, updatedApp: app)
        };

        // Then
        result
            .Should()
            .BeOfType<BadRequestObjectResult>();

        VerifyLogCounts(loggingBroker: loggingBroker, warning: 1);
    }

    [Theory]
    [InlineData(true, typeof(RedirectResult))]
    [InlineData(false, typeof(ViewResult))]
    public async Task SetupGet_WhenDependencyCompletes_ShouldReturnExpectedResultAndLogInformation(
        bool initialized,
        Type expectedType)
    {
        // Given
        Mock<IFirstTimeSetupStateOrchestrationService> service = new();
        Mock<ILoggingBroker> loggingBroker = new();

        service
            .Setup(expression: service => service.IsInitializedAsync(
                cancellationToken: It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: initialized);

        service
            .Setup(expression: service => service.NormalizeHost(
                host: It.IsAny<string>()))
            .Returns(value: "localhost");

        SetupController controller = new(
            setupStateService: service.Object,
            configuration: new CoreConfiguration
            {
                Packaging = new PackagingConfiguration()
            },
            loggingBroker: loggingBroker.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // When
        IActionResult result = await controller.Get(cancellationToken: CancellationToken.None);

        // Then
        result
            .Should()
            .BeOfType(expectedType: expectedType);

        VerifyLogCounts(loggingBroker: loggingBroker, information: 1);
    }

    [Theory]
    [InlineData(true, StatusCodes.Status401Unauthorized)]
    [InlineData(false, StatusCodes.Status500InternalServerError)]
    public async Task SetupGet_WhenDependencyFails_ShouldReturnClassifiedResponse(
        bool authenticationFailure,
        int expectedStatus)
    {
        // Given
        Exception exception = authenticationFailure
            ? new AuthenticationException(message: "authentication expired")
            : new InvalidOperationException(message: "database detail");

        Mock<IFirstTimeSetupStateOrchestrationService> service = new();
        Mock<ILoggingBroker> loggingBroker = new();

        service
            .Setup(expression: service => service.IsInitializedAsync(
                cancellationToken: It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception: exception);

        SetupController controller = new(
            setupStateService: service.Object,
            configuration: new CoreConfiguration(),
            loggingBroker: loggingBroker.Object);

        // When
        IActionResult result = await controller.Get(cancellationToken: CancellationToken.None);

        // Then
        ObjectResult response = result
            .Should()
            .BeAssignableTo<ObjectResult>()
            .Subject;

        response.StatusCode
            .Should()
            .Be(expected: expectedStatus);

        response.Value
            .Should()
            .NotBe(unexpected: "database detail");

        VerifyLogCounts(
            loggingBroker: loggingBroker,
            warning: authenticationFailure ? 1 : 0,
            error: authenticationFailure ? 0 : 1);
    }

    [Fact]
    public async Task SetupGet_WhenAuthorizationFails_ShouldReturnForbiddenAndWarn()
    {
        // Given
        Mock<IFirstTimeSetupStateOrchestrationService> service = new();
        Mock<ILoggingBroker> loggingBroker = new();

        service
            .Setup(expression: service => service.IsInitializedAsync(
                cancellationToken: It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception: new SecurityException(message: "access denied"));

        SetupController controller = new(
            setupStateService: service.Object,
            configuration: new CoreConfiguration(),
            loggingBroker: loggingBroker.Object);

        // When
        IActionResult result = await controller.Get(cancellationToken: CancellationToken.None);

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: StatusCodes.Status403Forbidden);

        VerifyLogCounts(loggingBroker: loggingBroker, warning: 1);
    }

    private static Mock<IAppAggregationService> CreateSuccessfulAppService(App app)
    {
        Mock<IAppAggregationService> service = new();

        service
            .Setup(expression: service => service.AddAppAsync(
                newApp: It.IsAny<App>()))
            .Returns(value: ValueTask.FromResult(result: app));

        service
            .Setup(expression: service => service.UpdateAppAsync(
                updatedApp: It.IsAny<App>()))
            .Returns(value: ValueTask.FromResult(result: app));

        service
            .Setup(expression: service => service.DeleteAppAsync(
                appId: It.IsAny<int>()))
            .Returns(value: ValueTask.FromResult(result: true));

        return service;
    }

    private static Mock<IAppAggregationService> CreateFailingAppService(Exception exception)
    {
        Mock<IAppAggregationService> service = new();

        service
            .Setup(expression: service => service.AddAppAsync(
                newApp: It.IsAny<App>()))
            .Returns(value: ValueTask.FromException<App>(exception: exception));

        service
            .Setup(expression: service => service.UpdateAppAsync(
                updatedApp: It.IsAny<App>()))
            .Returns(value: ValueTask.FromException<App>(exception: exception));

        service
            .Setup(expression: service => service.DeleteAppAsync(
                appId: It.IsAny<int>()))
            .Returns(value: ValueTask.FromException<bool>(exception: exception));

        return service;
    }

    private static int GetStatusCode(IActionResult result) =>
        result switch
        {
            ObjectResult objectResult => objectResult.StatusCode ?? StatusCodes.Status200OK,
            StatusCodeResult statusResult => statusResult.StatusCode,
            _ => StatusCodes.Status200OK
        };

    private static void VerifyLogCounts(
        Mock<ILoggingBroker> loggingBroker,
        int information = 0,
        int warning = 0,
        int error = 0)
    {
        loggingBroker.Invocations
            .Count(predicate: invocation =>
                invocation.Method.Name == nameof(ILoggingBroker.LogInformation))
            .Should()
            .Be(expected: information);

        loggingBroker.Invocations
            .Count(predicate: invocation =>
                invocation.Method.Name == nameof(ILoggingBroker.LogWarning))
            .Should()
            .Be(expected: warning);

        loggingBroker.Invocations
            .Count(predicate: invocation =>
                invocation.Method.Name == nameof(ILoggingBroker.LogError))
            .Should()
            .Be(expected: error);
    }
}