// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Security.Authentication;
using cCoder.Logging.Brokers.Loggings;
using FluentAssertions;
using HostedServices.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HostedServices.AcceptanceTests.Tests;

public sealed partial class HomeControllerContractTests
{
    [Fact]
    public void Get_WhenViewLoads_ShouldReturnViewAndLogInformation()
    {
        // Given
        TestLoggingBroker loggingBroker = new();
        HomeController controller = CreateController(loggingBroker: loggingBroker);

        // When
        IActionResult result = controller.Get();

        // Then
        result
            .Should()
            .BeOfType<ViewResult>();

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

    [Theory]
    [InlineData(true, StatusCodes.Status401Unauthorized)]
    [InlineData(false, StatusCodes.Status500InternalServerError)]
    public void Get_WhenViewLoadingFails_ShouldReturnClassifiedResponse(
        bool authenticationFailure,
        int expectedStatus)
    {
        // Given
        Exception exception = authenticationFailure
            ? new AuthenticationException(message: "authentication expired")
            : new InvalidOperationException(message: "internal detail");

        TestLoggingBroker loggingBroker = new(informationException: exception);
        HomeController controller = CreateController(loggingBroker: loggingBroker);

        // When
        IActionResult result = controller.Get();

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
            .NotBe(unexpected: "internal detail");

        loggingBroker.WarningCount
            .Should()
            .Be(expected: authenticationFailure ? 1 : 0);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: authenticationFailure ? 0 : 1);
    }

    private static HomeController CreateController(TestLoggingBroker loggingBroker)
    {
        HomeController controller = new(loggingBroker: loggingBroker)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
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