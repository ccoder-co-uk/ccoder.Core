// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.CodeAnalysis.Exposures;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class PackageArchitectureTests
{
    [Fact]
    public void CoreAssembly_WhenRuntimeDependenciesAreInspected_ReferencesContractsAndNotAnalyzer()
    {
        // Given
        Assembly coreAssembly = typeof(cCoder.Core.IServiceCollectionExtensions).Assembly;

        // When
        string[] referencedAssemblies = coreAssembly
            .GetReferencedAssemblies()
            .Select(selector: assemblyName => assemblyName.Name)
            .ToArray();

        // Then
        referencedAssemblies
            .Should()
            .Contain(expected: "cCoder.CodeAnalysis.Contracts");

        referencedAssemblies
            .Should()
            .NotContain(unexpected: "cCoder.CodeAnalysis");
    }

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

            ParameterInfo[] foundationParameters = parameters
                .Where(predicate: parameter => parameter.ParameterType.Namespace?
                    .Contains(
                        value: ".Services.Foundations.",
                        comparisonType: StringComparison.Ordinal) == true)
                .ToArray();

            foundationParameters
                .Should()
                .ContainSingle(
                    because: $"{processingType.Name} must cross one Foundation boundary");

            string foundationEntityName = foundationParameters[0]
                .ParameterType.Name
                .TrimStart(trimChars: ['I'])
                .Replace(oldValue: "Service", newValue: string.Empty);

            processingType.Name
                .Should()
                .Contain(
                    expected: foundationEntityName,
                    because: "the processing name must identify its Foundation entity");

            parameters
                .Except(second: foundationParameters)
                .Should()
                .OnlyContain(
                    predicate: parameter => typeof(IUtilityBroker)
                        .IsAssignableFrom(c: parameter.ParameterType),
                    because: "additional dependencies must be explicit utility brokers");
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