// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed class PackageArchitectureTests
{
    [Fact]
    public void PackageProcessingServices_WhenConstructed_UseOneMatchingFoundation()
    {
        Type[] processingTypes = typeof(cCoder.Core.IServiceCollectionExtensions)
            .Assembly
            .GetTypes()
            .Where(type => type.Namespace ==
                "cCoder.Core.Services.Processings.Packages")
            .Where(type => type.Name.EndsWith(
                value: "PackageProcessingService",
                comparisonType: StringComparison.Ordinal))
            .Where(type => !type.IsInterface)
            .ToArray();

        processingTypes.Should().NotBeEmpty();

        foreach (Type processingType in processingTypes)
        {
            ParameterInfo[] parameters = processingType
                .GetConstructors()
                .Single()
                .GetParameters();

            parameters.Should().ContainSingle(
                because: $"{processingType.Name} must cross one Foundation boundary");

            parameters[0].ParameterType.Namespace.Should().Be(
                expected: "cCoder.Core.Services.Foundations.Packages");
        }
    }

    [Fact]
    public void PackageAggregationServices_WhenImplemented_DoNotCallExternalPersistenceOrJsonApis()
    {
        string repositoryRoot = FindRepositoryRoot();
        string packageServicesPath = Path.Combine(
            repositoryRoot,
            "src",
            "cCoder.Core",
            "Services",
            "Aggregations",
            "Packages");

        string source = string.Join(
            separator: Environment.NewLine,
            values: Directory.GetFiles(packageServicesPath, "*.cs")
                .Select(File.ReadAllText));

        source.Should().NotContain("Microsoft.EntityFrameworkCore");
        source.Should().NotContain("System.Text.Json");
        source.Should().NotContain("ICoreContextFactory");
        source.Should().NotContain("DbContext");
        source.Should().NotContain("JsonSerializer");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new(AppContext.BaseDirectory);

        while (directory is not null &&
            !Directory.Exists(Path.Combine(directory.FullName, ".git")) &&
            !File.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException(
                message: "Could not locate the repository root.");
    }
}