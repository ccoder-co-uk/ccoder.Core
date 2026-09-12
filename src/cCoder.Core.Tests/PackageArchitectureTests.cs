// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class PackageArchitectureTests
{
    [Fact]
    public void PackageProcessingServices_WhenConstructed_UseOneMatchingFoundation()
    {
        // Given
        Assembly coreAssembly = typeof(cCoder.Core.IServiceCollectionExtensions).Assembly;

        // When
        Type[] processingTypes = coreAssembly
            .GetTypes()
            .Where(predicate: type => type.Namespace ==
                "cCoder.Core.Services.Processings.Packages")
            .Where(predicate: type => type.Name.EndsWith(
                value: "PackageProcessingService",
                comparisonType: StringComparison.Ordinal))
            .Where(predicate: type => !type.IsInterface)
            .ToArray();

        // Then
        processingTypes
            .Should()
            .NotBeEmpty();

        foreach (Type processingType in processingTypes)
        {
            ParameterInfo[] parameters = processingType
                .GetConstructors()
                .Single()
                .GetParameters();

            parameters
                .Should()
                .ContainSingle(
                    because: $"{processingType.Name} must cross one Foundation boundary");

            parameters[0]
                .ParameterType
                .Namespace
                .Should()
                .Be(expected: "cCoder.Core.Services.Foundations.Packages");
        }
    }

    [Fact]
    public void PackageAggregationServices_WhenImplemented_DoNotCallExternalPersistenceOrJsonApis()
    {
        // Given
        string repositoryRoot = FindRepositoryRoot();

        string packageServicesPath = Path.Combine(
            paths:
            [
                repositoryRoot,
                "src",
                "cCoder.Core",
                "Services",
                "Aggregations",
                "Packages",
            ]);

        // When
        string source = string.Join(
            separator: Environment.NewLine,
            values: Directory
                .GetFiles(
                    path: packageServicesPath,
                    searchPattern: "*.cs")
                .Select(selector: file => File.ReadAllText(path: file)));

        // Then
        source
            .Should()
            .NotContain(unexpected: "Microsoft.EntityFrameworkCore");

        source
            .Should()
            .NotContain(unexpected: "System.Text.Json");

        source
            .Should()
            .NotContain(unexpected: "ICoreContextFactory");

        source
            .Should()
            .NotContain(unexpected: "DbContext");

        source
            .Should()
            .NotContain(unexpected: "JsonSerializer");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new(AppContext.BaseDirectory);

        while (directory is not null &&
            !File.Exists(path: Path.Combine(
                paths:
                [
                    directory.FullName,
                    "src",
                    "cCoder.Core",
                    "cCoder.Core.csproj",
                ])))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException(
                message: "Could not locate the repository root.");
    }
}