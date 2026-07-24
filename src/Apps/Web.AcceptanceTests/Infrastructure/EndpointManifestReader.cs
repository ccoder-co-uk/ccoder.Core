// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;


namespace Web.AcceptanceTests.Infrastructure;

internal static class EndpointManifestReader
{
    public static string[] LoadExpectedRoutes()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string resourceName = assembly.GetManifestResourceNames()
            .Single(predicate: name => name.EndsWith(value: "Assets.EndpointManifest.txt",comparisonType: StringComparison.Ordinal));

        using Stream stream = assembly.GetManifestResourceStream(name: resourceName)
            ?? throw new InvalidOperationException("Endpoint manifest resource could not be loaded.");

        using StreamReader reader = new(stream);

        return reader.ReadToEnd()
            .Split(separator: ['\r', '\n'],options: StringSplitOptions.RemoveEmptyEntries)
            .Select(selector: static line => line.Trim())
            .Where(predicate: line => !line.StartsWith(value: "#",comparisonType: StringComparison.Ordinal))
            .OrderBy(keySelector: line => line,comparer: StringComparer.Ordinal)
            .ToArray();
    }
}