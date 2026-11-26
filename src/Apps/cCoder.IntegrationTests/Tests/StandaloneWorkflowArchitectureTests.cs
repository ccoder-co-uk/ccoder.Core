// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class StandaloneWorkflowArchitectureTests
{
    [Fact]
    public void StandaloneHost_WhenComposed_DelegatesToWorkflowEngineWithoutDataRegistration()
    {
        // Given

        // When
        string source = ReadStandaloneWorkflowSource();

        // Then
        source.Should()
            .Contain(expected: "AddWorkflowEngineHostedServices");

        source.Should()
            .Contain(expected: "IFlowRunner flowRunner");

        source.Should()
            .Contain(expected: "IWorkflowScriptExecutionService scriptExecutionService");

        source.Should()
            .NotContain(unexpected: "AddData(");
    }

    [Fact]
    public void StandaloneHost_WhenHandlingRequests_DoesNotCallTheWebApiDirectly()
    {
        // Given

        // When
        string source = ReadStandaloneWorkflowSource();

        // Then
        source.Should()
            .NotContain(unexpected: "new HttpClient(");

        source.Should()
            .NotContain(unexpected: ".GetAsync(");

        source.Should()
            .NotContain(unexpected: ".SendAsync(");
    }

    private static string ReadStandaloneWorkflowSource(
        [CallerFilePath] string callerFilePath = "")
    {
        string appsDirectory = Path.GetFullPath(
            path: Path.Combine(
                path1: Path.GetDirectoryName(path: callerFilePath),
                path2: "..",
                path3: ".."));

        string sourcePath = Path.Combine(
            paths:
            [
                appsDirectory,
                "Workflow",
                "Program.cs"
            ]);

        return File.ReadAllText(path: sourcePath);
    }
}