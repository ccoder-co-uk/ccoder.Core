using cCoder.Core.Setup;
using cCoder.Data.Models;
using cCoder.Data.Models.Packaging;

namespace Web.Services.Setup;

internal sealed class FirstTimeSetupAssetService
{
    private readonly BaselineAssetCatalog catalog = new();

    public string LoadDefaultAppConfig()
        => catalog.LoadDefaultAppConfig();

    public Package[] LoadPackages() =>
        catalog.LoadPackages();

    public T[] LoadPackageItems<T>(string packageName, string itemType)
        => catalog.LoadPackageItems<T>(packageName, itemType);

    public CommonObject[] LoadCommonObjects() =>
        catalog.LoadCommonObjects();
}
