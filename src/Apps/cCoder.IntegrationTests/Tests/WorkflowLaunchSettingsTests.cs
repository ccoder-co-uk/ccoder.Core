using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed class WorkflowLaunchSettingsTests
{
    [Fact]
    public void WorkflowProfile_ShouldUseFunctionsHostArguments()
    {
        string repositoryRoot = FindRepositoryRoot();
        string launchSettingsPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "Workflow",
            "Properties",
            "launchSettings.json");

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(launchSettingsPath));

        JsonElement profile = document.RootElement
            .GetProperty("profiles")
            .GetProperty("Workflow");

        profile.GetProperty("commandName").GetString().Should().Be("Project");
        profile.GetProperty("commandLineArgs").GetString().Should().Be("--port 7071");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "cCoder.Core.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the ccoder.Core repository root.");
    }
}
