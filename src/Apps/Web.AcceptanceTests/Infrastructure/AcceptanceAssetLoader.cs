// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;


namespace Web.AcceptanceTests.Infrastructure;

internal static class AcceptanceAssetLoader
{
    public static string AssetsDirectory =>
        Path.Combine(path1: AppContext.BaseDirectory,path2: "Assets");

    public static string LoadText(string fileName)
    {
        string path = Path.Combine(path1: AssetsDirectory,path2: fileName);

        if (!File.Exists(path: path))
        {
            throw new FileNotFoundException($"Acceptance asset was not found: {path}", path);
        }

        return File.ReadAllText(path: path);
    }

    public static JsonDocument LoadJson(string fileName)
    {
        return JsonDocument.Parse(json: LoadText(fileName: fileName));
    }
}