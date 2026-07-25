// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class WorkflowLaunchSettingsTests
{
    [Fact]
    public void WorkflowProfile_ShouldUseFunctionsHostArguments()
    {
        // Given
        string repositoryRoot = FindRepositoryRoot();

        string launchSettingsPath = Path.Combine(
            paths:
            [
                repositoryRoot,
                "src",
                "Apps",
                "Workflow",
                "Properties",
                "launchSettings.json"
            ]);

        using JsonDocument document = JsonDocument.Parse(json: File.ReadAllText(path: launchSettingsPath));

        // When
        JsonElement profile = document.RootElement
            .GetProperty(propertyName: "profiles")
            .GetProperty(propertyName: "Workflow");

        // Then
        profile.GetProperty(propertyName: "commandName")
            .GetString()
            .Should()
            .Be(expected: "Project");

        profile.GetProperty(propertyName: "commandLineArgs")
            .GetString()
            .Should()
            .Be(expected: "--port 7071");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(path: Path.Combine(path1: directory.FullName,path2: "src",path3: "cCoder.Core.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the ccoder.Core repository root.");
    }
}