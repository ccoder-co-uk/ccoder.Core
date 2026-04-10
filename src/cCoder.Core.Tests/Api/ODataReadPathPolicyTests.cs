using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed class ODataReadPathPolicyTests
{
    [Fact]
    public void EntityControllers_GetById_ShouldQueryThroughFilteredGetAll()
    {
        string[] controllerFiles = GetControllerFiles();
        List<string> violations = [];

        foreach (string file in controllerFiles)
        {
            string source = File.ReadAllText(file);

            if (!source.Contains("[FromRoute]", StringComparison.Ordinal) ||
                !source.Contains("ODataQueryOptions", StringComparison.Ordinal))
            {
                continue;
            }

            string routeGetBody = ExtractMethodBody(source, "public IActionResult Get([FromRoute]");
            if (string.IsNullOrWhiteSpace(routeGetBody))
            {
                continue;
            }

            if (!routeGetBody.Contains("SingleResult.Create(", StringComparison.Ordinal) ||
                !routeGetBody.Contains(".GetAll(", StringComparison.Ordinal) ||
                Regex.IsMatch(routeGetBody, @"\b[a-zA-Z_][a-zA-Z0-9_]*\.Get\(key\)"))
            {
                violations.Add(RelativeToRepository(file));
            }
        }

        violations.Should().BeEmpty("entity OData Get(id) actions should read through filtered GetAll() so OData applies to the query root");
    }

    [Fact]
    public void EntityControllers_CollectionReads_ShouldNotIgnoreFilters()
    {
        string[] controllerFiles = GetControllerFiles();
        List<string> violations = [];

        foreach (string file in controllerFiles)
        {
            string source = File.ReadAllText(file);

            if (!source.Contains("ODataQueryOptions", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (string methodSignature in new[]
                     {
                         "public IActionResult GetAll(ODataQueryOptions<",
                         "public IActionResult Get(ODataQueryOptions<"
                     })
            {
                string methodBody = ExtractMethodBody(source, methodSignature);
                if (string.IsNullOrWhiteSpace(methodBody))
                {
                    continue;
                }

                if (methodBody.Contains("GetAll(true", StringComparison.Ordinal) ||
                    methodBody.Contains("GetAll(ignoreFilters: true", StringComparison.Ordinal))
                {
                    violations.Add(RelativeToRepository(file));
                    break;
                }
            }
        }

        violations.Should().BeEmpty("HTTP GET exposure points should remain filtered and must not bypass query filters");
    }

    private static string RepositoryRoot =>
        repositoryRoot ??= FindRepositoryRoot();

    private static string repositoryRoot;

    private static string[] GetControllerFiles() =>
        Directory.GetFiles(Path.Combine(RepositoryRoot, "src"), "*Controller.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}Exposures{Path.DirectorySeparatorChar}Controllers{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private static string ExtractMethodBody(string source, string signatureStart)
    {
        int signatureIndex = source.IndexOf(signatureStart, StringComparison.Ordinal);
        if (signatureIndex < 0)
        {
            return string.Empty;
        }

        int arrowIndex = source.IndexOf("=>", signatureIndex, StringComparison.Ordinal);
        int braceIndex = source.IndexOf('{', signatureIndex);

        if (arrowIndex >= 0 && (braceIndex < 0 || arrowIndex < braceIndex))
        {
            int statementEnd = source.IndexOf(';', arrowIndex);
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
            if (Directory.Exists(Path.Combine(current.FullName, "src")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root for source-policy tests.");
    }

    private static string RelativeToRepository(string path) =>
        Path.GetRelativePath(RepositoryRoot, path).Replace(Path.DirectorySeparatorChar, '/');
}
