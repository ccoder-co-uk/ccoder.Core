// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed partial class ODataReadPathPolicyTests
{
    [Fact]
    public void EntityControllers_GetById_ShouldQueryThroughFilteredGetAll()
    {
        // Given
        string[] controllerFiles = GetControllerFiles();
        List<string> violations = [];

        // When
        foreach (string file in controllerFiles)
        {
            string source = File.ReadAllText(path: file);

            if (!source.Contains(
                    value: "[FromRoute]",
                    comparisonType: StringComparison.Ordinal)
                || !source.Contains(
                    value: "ODataQueryOptions",
                    comparisonType: StringComparison.Ordinal))
            {
                continue;
            }

            string routeGetBody = ExtractMethodBody(
                source: source,
                signatureStart: "public IActionResult Get([FromRoute]");

            if (string.IsNullOrWhiteSpace(value: routeGetBody))
            {
                continue;
            }

            if (!routeGetBody.Contains(value: "SingleResult.Create(",comparisonType: StringComparison.Ordinal) ||
                !routeGetBody.Contains(value: ".GetAll(",comparisonType: StringComparison.Ordinal) ||
                Regex.IsMatch(input: routeGetBody,pattern: @"\b[a-zA-Z_][a-zA-Z0-9_]*\.Get\(key\)"))
            {
                violations.Add(item: RelativeToRepository(path: file));
            }
        }

        // Then
        violations
            .Should()
            .BeEmpty(because: "entity OData Get(id) actions should read through filtered GetAll() so OData applies to the query root");
    }

    [Fact]
    public void EntityControllers_CollectionReads_ShouldNotIgnoreFilters()
    {
        // Given
        string[] controllerFiles = GetControllerFiles();
        List<string> violations = [];

        // When
        foreach (string file in controllerFiles)
        {
            string source = File.ReadAllText(path: file);

            if (!source.Contains(value: "ODataQueryOptions",comparisonType: StringComparison.Ordinal))
            {
                continue;
            }

            foreach (string methodSignature in new[]
                     {
                         "public IActionResult GetAll(ODataQueryOptions<",
                         "public IActionResult Get(ODataQueryOptions<"
                     })
            {
                string methodBody = ExtractMethodBody(source: source,signatureStart: methodSignature);

                if (string.IsNullOrWhiteSpace(value: methodBody))
                {
                    continue;
                }

                if (methodBody.Contains(value: "GetAll(true",comparisonType: StringComparison.Ordinal) ||
                    methodBody.Contains(value: "GetAll(ignoreFilters: true",comparisonType: StringComparison.Ordinal))
                {
                    violations.Add(item: RelativeToRepository(path: file));
                    break;
                }
            }
        }

        // Then
        violations
            .Should()
            .BeEmpty(because: "HTTP GET exposure points should remain filtered and must not bypass query filters");
    }

    private static string RepositoryRoot =>
        repositoryRoot ??= FindRepositoryRoot();

    private static string repositoryRoot;

    private static string[] GetControllerFiles() =>
        Directory.GetFiles(path: Path.Combine(path1: RepositoryRoot,path2: "src"),searchPattern: "*Controller.cs",searchOption: SearchOption.AllDirectories)
            .Where(predicate: path => !path.Contains(value: $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",comparisonType: StringComparison.OrdinalIgnoreCase))
            .Where(predicate: path => !path.Contains(value: $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",comparisonType: StringComparison.OrdinalIgnoreCase))
            .Where(predicate: path => path.Contains(value: $"{Path.DirectorySeparatorChar}Exposures{Path.DirectorySeparatorChar}Controllers{Path.DirectorySeparatorChar}",comparisonType: StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private static string ExtractMethodBody(string source, string signatureStart)
    {
        int signatureIndex = source.IndexOf(value: signatureStart,comparisonType: StringComparison.Ordinal);

        if (signatureIndex < 0)
        {
            return string.Empty;
        }

        int arrowIndex = source.IndexOf(value: "=>",startIndex: signatureIndex,comparisonType: StringComparison.Ordinal);
        int braceIndex = source.IndexOf(value: '{',startIndex: signatureIndex);

        if (arrowIndex >= 0 && (braceIndex < 0 || arrowIndex < braceIndex))
        {
            int statementEnd = source.IndexOf(value: ';',startIndex: arrowIndex);

            return statementEnd < 0
                ? source[arrowIndex..]
                : source[arrowIndex..statementEnd];
        }

        if (braceIndex < 0)
        {
            return string.Empty;
        }

        int depth = 0;

        for (int index = braceIndex; index < source.Length; index++)
        {
            if (source[index] == '{')
            {
                depth++;
            }
            else if (source[index] == '}')
            {
                depth--;

                if (depth == 0)
                {
                    return source[(braceIndex + 1)..index];
                }
            }
        }

        return string.Empty;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (Directory.Exists(path: Path.Combine(path1: current.FullName,path2: "src")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root for source-policy tests.");
    }

    private static string RelativeToRepository(string path) =>
        Path.GetRelativePath(relativeTo: RepositoryRoot,path: path)
            .Replace(oldChar: Path.DirectorySeparatorChar,newChar: '/');
}