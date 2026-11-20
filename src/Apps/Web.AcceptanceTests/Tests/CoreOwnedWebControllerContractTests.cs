// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using cCoder.Core.Brokers.Loggings;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Controllers.Api;
using Xunit;

namespace Web.AcceptanceTests.Tests;

public sealed partial class CoreOwnedWebControllerContractTests
{
    [Fact]
    public async Task Actions_WhenDependencyRejectsAuthentication_ShouldReturn401AndWarnOnce()
    {
        // Given
        Exception exception = new AuthenticationException(message: "authentication expired");

        // When
        Task assertion = AssertFailureContractAsync(
            exception: exception,
            expectedStatus: StatusCodes.Status401Unauthorized,
            expectedWarningCount: 1,
            expectedErrorCount: 0);

        // Then
        await assertion;
    }

    [Fact]
    public async Task Actions_WhenDependencyFailsUnexpectedly_ShouldReturnDetailFree500AndLogErrorOnce()
    {
        // Given
        Exception exception = new InvalidOperationException(message: "database detail");

        // When
        Task assertion = AssertFailureContractAsync(
            exception: exception,
            expectedStatus: StatusCodes.Status500InternalServerError,
            expectedWarningCount: 0,
            expectedErrorCount: 1);

        // Then
        await assertion;
    }

    [Fact]
    public async Task Actions_WhenDependencyRejectsAuthorization_ShouldReturn403AndWarnOnce()
    {
        // Given
        Exception exception = new SecurityException(message: "access denied");

        // When
        Task assertion = AssertFailureContractAsync(
            exception: exception,
            expectedStatus: StatusCodes.Status403Forbidden,
            expectedWarningCount: 1,
            expectedErrorCount: 0);

        // Then
        await assertion;
    }

    [Fact]
    public void ApiTime_WhenRequestCompletes_ShouldReturnOkAndLogInformation()
    {
        // Given
        TestLoggingBroker loggingBroker = new();

        ApiRootController controller = (ApiRootController)CreateController(
            controllerType: typeof(ApiRootController),
            exception: null,
            loggingBroker: loggingBroker);

        // When
        IActionResult result = controller.GetTime();

        // Then
        result
            .Should()
            .BeOfType<OkObjectResult>();

        loggingBroker.InformationCount
            .Should()
            .Be(expected: 1);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: 0);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: 0);
    }

    [Fact]
    public async Task ApiActions_WhenDependenciesComplete_ShouldReturnSuccessAndLogInformation()
    {
        // Given
        (Type Controller, string Action)[] actions =
        [
            (typeof(ApiRootController), nameof(ApiRootController.Get)),
            (typeof(ApiRootController), nameof(ApiRootController.Post)),
            (typeof(ApiRootController), nameof(ApiRootController.Put)),
            (typeof(CacheController), nameof(CacheController.GetRefreshCache)),
            (typeof(MetadataController), nameof(MetadataController.GetMetadata)),
            (typeof(ScriptController), nameof(ScriptController.PostExecuteScript))
        ];

        // When
        foreach ((Type controllerType, string actionName) in actions)
        {
            TestLoggingBroker loggingBroker = new();

            Controller controller = CreateController(
                controllerType: controllerType,
                exception: null,
                loggingBroker: loggingBroker);

            MethodInfo action = controllerType.GetMethod(name: actionName);

            object[] arguments = action.GetParameters()
                .Select(selector: parameter => parameter.HasDefaultValue
                    ? parameter.DefaultValue
                    : GetDefaultValue(type: parameter.ParameterType))
                .ToArray();

            object invocationResult = action.Invoke(obj: controller, parameters: arguments);

            IActionResult result = invocationResult is Task<IActionResult> task
                ? await task
                : (IActionResult)invocationResult;

            // Then
            result
                .Should()
                .NotBeNull();

            loggingBroker.InformationCount
                .Should()
                .Be(
                    expected: 1,
                    because: $"{controllerType.Name}.{actionName} should log success");

            loggingBroker.WarningCount
                .Should()
                .Be(expected: 0);

            loggingBroker.ErrorCount
                .Should()
                .Be(expected: 0);
        }
    }

    [Theory]
    [InlineData(true, StatusCodes.Status401Unauthorized, 1, 0)]
    [InlineData(false, StatusCodes.Status500InternalServerError, 0, 1)]
    public void ApiTime_WhenSuccessLoggingFails_ShouldReturnClassifiedResponse(
        bool authenticationFailure,
        int expectedStatus,
        int expectedWarningCount,
        int expectedErrorCount)
    {
        // Given
        Exception exception = authenticationFailure
            ? new AuthenticationException(message: "authentication expired")
            : new InvalidOperationException(message: "internal detail");

        TestLoggingBroker loggingBroker = new(informationException: exception);

        ApiRootController controller = (ApiRootController)CreateController(
            controllerType: typeof(ApiRootController),
            exception: null,
            loggingBroker: loggingBroker);

        // When
        IActionResult result = controller.GetTime();

        // Then
        AssertResponse(
            result: result,
            expectedStatus: expectedStatus,
            expectedWarningCount: expectedWarningCount,
            expectedErrorCount: expectedErrorCount,
            loggingBroker: loggingBroker,
            expectedInformationCount: 1);
    }

    private static async Task AssertFailureContractAsync(
        Exception exception,
        int expectedStatus,
        int expectedWarningCount,
        int expectedErrorCount)
    {
        (Type Controller, string Action)[] actions =
        [
            (typeof(ApiRootController), nameof(ApiRootController.Get)),
            (typeof(ApiRootController), nameof(ApiRootController.Post)),
            (typeof(ApiRootController), nameof(ApiRootController.Put)),
            (typeof(CacheController), nameof(CacheController.GetRefreshCache)),
            (typeof(MetadataController), nameof(MetadataController.GetMetadata)),
            (typeof(ScriptController), nameof(ScriptController.PostExecuteScript)),
            (typeof(HomeController), nameof(HomeController.Index))
        ];

        foreach ((Type controllerType, string actionName) in actions)
        {
            TestLoggingBroker loggingBroker = new();

            Controller controller = CreateController(
                controllerType: controllerType,
                exception: exception,
                loggingBroker: loggingBroker);

            MethodInfo action = controllerType.GetMethod(name: actionName);

            object[] arguments = action.GetParameters()
                .Select(selector: parameter => parameter.HasDefaultValue
                    ? parameter.DefaultValue
                    : GetDefaultValue(type: parameter.ParameterType))
                .ToArray();

            object invocationResult = action.Invoke(obj: controller, parameters: arguments);

            IActionResult result = invocationResult is Task<IActionResult> task
                ? await task
                : (IActionResult)invocationResult;

            AssertResponse(
                result: result,
                expectedStatus: expectedStatus,
                expectedWarningCount: expectedWarningCount,
                expectedErrorCount: expectedErrorCount,
                loggingBroker: loggingBroker);
        }
    }

    private static Controller CreateController(
        Type controllerType,
        Exception exception,
        TestLoggingBroker loggingBroker)
    {
        ConstructorInfo constructor = controllerType
            .GetConstructors()
            .Single();

        object[] dependencies = constructor
            .GetParameters()
            .Select(selector: parameter => parameter.ParameterType == typeof(ILoggingBroker)
                ? loggingBroker
                : CreateThrowingProxy(interfaceType: parameter.ParameterType, exception: exception))
            .ToArray();

        Controller controller = (Controller)constructor.Invoke(parameters: dependencies);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private static object CreateThrowingProxy(Type interfaceType, Exception exception)
    {
        object proxy = DispatchProxy.Create(interfaceType: interfaceType, proxyType: typeof(ThrowingProxy));
        ((ThrowingProxy)proxy).Exception = exception;

        return proxy;
    }

    private static object GetDefaultValue(Type type) =>
        type.IsValueType
            ? Activator.CreateInstance(type: type)
            : null;

    private static void AssertResponse(
        IActionResult result,
        int expectedStatus,
        int expectedWarningCount,
        int expectedErrorCount,
        TestLoggingBroker loggingBroker,
        int expectedInformationCount = 0)
    {
        ObjectResult response = result
            .Should()
            .BeAssignableTo<ObjectResult>()
            .Subject;

        response.StatusCode
            .Should()
            .Be(expected: expectedStatus);

        response.Value
            .Should()
            .NotBe(unexpected: "database detail")
            .And
            .NotBe(unexpected: "internal detail");

        loggingBroker.InformationCount
            .Should()
            .Be(expected: expectedInformationCount);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: expectedWarningCount);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: expectedErrorCount);
    }

    private class ThrowingProxy : DispatchProxy
    {
        public Exception Exception { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (Exception is not null)
            {
                throw Exception;
            }

            Type returnType = targetMethod.ReturnType;

            if (returnType == typeof(void))
            {
                return null;
            }

            if (returnType == typeof(string))
            {
                return "{}";
            }

            if (returnType == typeof(Task))
            {
                return Task.CompletedTask;
            }

            if (returnType.IsGenericType &&
                returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                Type resultType = returnType.GetGenericArguments()[0];
                object result = GetDefaultValue(type: resultType);

                return typeof(Task)
                    .GetMethod(name: nameof(Task.FromResult))
                    .MakeGenericMethod(typeArguments: resultType)
                    .Invoke(obj: null, parameters: [result]);
            }

            return GetDefaultValue(type: returnType);
        }
    }

    private sealed class TestLoggingBroker(Exception informationException = null)
        : ILoggingBroker
    {
        public int InformationCount { get; private set; }

        public int WarningCount { get; private set; }

        public int ErrorCount { get; private set; }

        public bool IsDebugEnabled() =>
            false;

        public bool IsInformationEnabled() =>
            true;

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

        public void LogWarning(
            Exception exception,
            string message,
            params object[] args) =>
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