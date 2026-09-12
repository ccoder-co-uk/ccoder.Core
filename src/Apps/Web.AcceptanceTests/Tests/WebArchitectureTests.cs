// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace Web.AcceptanceTests.Tests;

public sealed partial class WebArchitectureTests
{
    [Fact]
    public void ApiContextBroker_WhenComposed_UsesApiContextDependency()
    {
        // Given

        // When
        string source = ReadWebSource(
            paths: ["Brokers", "Api", "ApiContextBroker.cs"]);

        // Then
        source.Should()
            .Contain(expected: "ApiContextDependency apiContextDependency");

        source.Should()
            .NotContain(unexpected: "IEnumerable<ApiInfo> apiInfos");
    }

    [Fact]
    public void HomeController_WhenUsingRequestApis_UsesHomeSessionManager()
    {
        // Given

        // When
        string source = ReadWebSource(
            paths: ["Controllers", "HomeController.cs"]);

        // Then
        source.Should()
            .NotContain(unexpected: "Response.HttpContext.Abort()");

        source.Should()
            .NotContain(unexpected: "Url.IsLocalUrl(");

        source.Should()
            .Contain(expected: "homeSessionManager.AbortRequest");

        source.Should()
            .Contain(expected: "homeSessionManager.IsLocalUrl");
    }

    [Fact]
    public void HomeSessionProcessing_WhenComposed_UsesOnlyMatchingFoundation()
    {
        // Given

        // When
        string source = ReadWebSource(
            paths:
            [
                "Services",
                "Processings",
                "HomeSessionProcessingService.cs"
            ]);

        // Then
        source.Should()
            .Contain(expected: "IHomeSessionService homeSessionService");

        source.Should()
            .NotContain(unexpected: "RequestServices");

        source.Should()
            .NotContain(unexpected: "context.Session");

        source.Should()
            .NotContain(unexpected: "context.Request.Query");
    }

    [Fact]
    public void ApiScriptServices_WhenAcceptingRequest_UseFullModelParameterName()
    {
        // Given
        string managerSource = ReadWebSource(
            paths: ["Exposures", "ApiScriptManager.cs"]);

        string orchestrationSource = ReadWebSource(
            paths:
            [
                "Services",
                "Orchestrations",
                "Api",
                "ApiScriptOrchestrationService.cs"
            ]);

        // When
        string source = managerSource + orchestrationSource;

        // Then
        source.Should()
            .Contain(expected: "ApiScriptRequest apiScriptRequest");

        source.Should()
            .NotContain(unexpected: "ApiScriptRequest request");
    }

    private static string ReadWebSource(
        string[] paths,
        [CallerFilePath] string callerFilePath = "")
    {
        string appsDirectory = Path.GetFullPath(
            path: Path.Combine(
                path1: Path.GetDirectoryName(path: callerFilePath),
                path2: "..",
                path3: ".."));

        string sourcePath = Path.Combine(
            paths: [appsDirectory, "Web", .. paths]);

        return File.ReadAllText(path: sourcePath);
    }
}