// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.Data.Extensions;
using Newtonsoft.Json;

namespace cCoder.Core.Services.Processings.Setup;

internal sealed partial class BaselineAssetCatalogProcessingService
    : IBaselineAssetCatalogProcessingService
{
    private const string ResourcePrefix =
        "cCoder.Core.Exposures.Setup.Assets.";

    private readonly Assembly assembly;
    private readonly JsonSerializerSettings settings =
        ObjectExtensions.GetJSONSettings();

    public BaselineAssetCatalogProcessingService()
        : this(typeof(BaselineAssetCatalogProcessingService).Assembly)
    {
    }

    internal BaselineAssetCatalogProcessingService(Assembly assembly) =>
        this.assembly = assembly;

    public string LoadDefaultAppConfig() =>
        TryCatch(operation: () =>
            LoadText(relativePath: "DefaultAppConfig.json"));

    public byte[] LoadAssetBytes(string relativePath) =>
        TryCatch(operation: () =>
        {
            ValidateRelativePathOnLoad(relativePath: relativePath);

            return LoadBytes(relativePath: relativePath);
        });

    public string[] LoadDmsAssetPaths() =>
        TryCatch(operation: () =>
            JsonConvert.DeserializeObject<string[]>(
                value: LoadText(
                    relativePath: Path.Combine(
                        path1: "Baseline",
                        path2: "DMS",
                        path3: "BaselineDmsAssets.json")),
                settings: settings) ?? []);

    private string LoadText(string relativePath)
    {
        using Stream stream =
            LoadResourceStream(relativePath: relativePath);

        using StreamReader reader = new(stream);

        return reader.ReadToEnd();
    }

    private byte[] LoadBytes(string relativePath)
    {
        using Stream stream =
            LoadResourceStream(relativePath: relativePath);

        using MemoryStream memoryStream = new();
        stream.CopyTo(destination: memoryStream);

        return memoryStream.ToArray();
    }

    private Stream LoadResourceStream(string relativePath)
    {
        string resourceName =
            $"{ResourcePrefix}{relativePath.Replace(oldChar: '\\', newChar: '.')
                .Replace(oldChar: '/', newChar: '.')}";

        string normalizedResourceName =
            resourceName.Replace(oldChar: ' ', newChar: '_');

        return assembly.GetManifestResourceStream(name: resourceName)
            ?? assembly.GetManifestResourceStream(
                name: normalizedResourceName)
            ?? throw new FileNotFoundException(
                $"Baseline asset was not found: {resourceName}",
                resourceName);
    }
}