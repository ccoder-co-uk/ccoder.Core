// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class WorkflowFunctionsArchitectureTests
{
    [Fact]
    public void WorkflowFunctionsProcessing_WhenComposed_UsesOnlyMatchingFoundation()
    {
        // Given

        // When
        string source = ReadWorkflowFunctionsProcessingSource();

        // Then
        source.Should()
            .Contain(expected: "IWorkflowFunctionsService workflowFunctionsService");

        source.Should()
            .NotContain(unexpected: "IFlowRunner flowRunner");

        source.Should()
            .NotContain(unexpected: "IWorkflowScriptExecutionService scriptExecutionService");

        source.Should()
            .NotContain(unexpected: "ILoggingBroker loggingBroker");
    }

    [Fact]
    public void WorkflowFunctionsProcessing_WhenCallingExternalApis_DoesNotCallThemDirectly()
    {
        // Given

        // When
        string source = ReadWorkflowFunctionsProcessingSource();

        // Then
        source.Should()
            .NotContain(unexpected: "JsonConvert.");

        source.Should()
            .NotContain(unexpected: ".CreateResponse(");

        source.Should()
            .NotContain(unexpected: ".WriteStringAsync(");
    }

    private static string ReadWorkflowFunctionsProcessingSource(
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
                "Services",
                "Processings",
                "WorkflowFunctions",
                "WorkflowFunctionsProcessingService.cs"
            ]);

        return File.ReadAllText(path: sourcePath);
    }
}