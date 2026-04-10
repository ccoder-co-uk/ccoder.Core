using System.Reflection;


namespace Web.AcceptanceTests.Infrastructure;

internal static class EndpointManifestReader
{
    public static string[] LoadExpectedRoutes()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames()
            .Single(name => name.EndsWith("Assets.EndpointManifest.txt", StringComparison.Ordinal));

        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Endpoint manifest resource could not be loaded.");
        using StreamReader reader = new(stream);

        return reader.ReadToEnd()
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(static line => line.Trim())
            .Where(line => !line.StartsWith("#", StringComparison.Ordinal))
            .OrderBy(line => line, StringComparer.Ordinal)
            .ToArray();
    }
}


