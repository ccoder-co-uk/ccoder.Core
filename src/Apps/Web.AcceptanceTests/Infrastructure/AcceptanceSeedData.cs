using cCoder.Core.Setup;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;

namespace Web.AcceptanceTests.Infrastructure;

internal static class AcceptanceSeedData
{
    private static readonly BaselineAssetCatalog Catalog = new();

    public static string LoadDefaultAppConfig() =>
        Catalog.LoadDefaultAppConfig();

    public static Package[] LoadExportPackages() =>
        Catalog.LoadPackages();

    public static T[] LoadPackageItems<T>(string packageName, string itemType) =>
        Catalog.LoadPackageItems<T>(packageName, itemType);

    public static CommonObject[] LoadCommonObjects() =>
        Catalog.LoadCommonObjects();
}






