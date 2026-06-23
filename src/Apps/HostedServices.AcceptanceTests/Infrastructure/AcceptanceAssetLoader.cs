using System.Text.Json;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal static class AcceptanceAssetLoader
{
    public static string AssetsDirectory =>
        Path.Combine(AppContext.BaseDirectory, "Assets");

    public static string LoadText(string fileName)
    {
        string path = Path.Combine(AssetsDirectory, fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Acceptance asset was not found: {path}", path);

        return File.ReadAllText(path);
    }

    public static JsonDocument LoadJson(string fileName) =>
        JsonDocument.Parse(LoadText(fileName));
}
