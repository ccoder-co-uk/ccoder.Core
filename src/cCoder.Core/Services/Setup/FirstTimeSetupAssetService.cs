using cCoder.Core.Exposures.Setup;
using cCoder.Data.Models;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Setup;

internal sealed class FirstTimeSetupAssetService
{
    private readonly BaselineAssetCatalog catalog = new();

    public string LoadDefaultAppConfig()
        => catalog.LoadDefaultAppConfig();

    public byte[] LoadAssetBytes(string relativePath)
        => catalog.LoadAssetBytes(relativePath);

    public string[] LoadDmsAssetPaths()
        => catalog.LoadDmsAssetPaths();

    public Package[] LoadPackages() =>
        catalog.LoadPackages();

    public T[] LoadPackageItems<T>(string packageName, string itemType)
        => catalog.LoadPackageItems<T>(packageName, itemType);

    public CommonObject[] LoadCommonObjects() =>
        catalog.LoadCommonObjects();
}
