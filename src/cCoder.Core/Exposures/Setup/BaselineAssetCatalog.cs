// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.Core.Services.Processings.Setup;
using cCoder.Data.Models;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Exposures.Setup;

public sealed class BaselineAssetCatalog
{
    private static readonly IBaselinePackageCatalogProcessingService
        packageProcessingService =
            new BaselinePackageCatalogProcessingService();

    private static readonly IBaselineCommonObjectCatalogProcessingService
        commonObjectProcessingService =
            new BaselineCommonObjectCatalogProcessingService();

    private readonly IBaselineAssetCatalogProcessingService processingService;

    public BaselineAssetCatalog()
        : this(new BaselineAssetCatalogProcessingService())
    {
    }

    internal BaselineAssetCatalog(Assembly assembly)
        : this(new BaselineAssetCatalogProcessingService(
            assembly: assembly))
    {
    }

    private BaselineAssetCatalog(
        IBaselineAssetCatalogProcessingService processingService) =>
        this.processingService = processingService;

    public string LoadDefaultAppConfig() =>
        processingService.LoadDefaultAppConfig();

    public byte[] LoadAssetBytes(string relativePath) =>
        processingService.LoadAssetBytes(relativePath: relativePath);

    public string[] LoadDmsAssetPaths() =>
        processingService.LoadDmsAssetPaths();

    public Package[] LoadCoreReviewPackages() =>
        packageProcessingService.LoadCoreReviewPackages();

    public Package[] LoadPackages() =>
        packageProcessingService.LoadPackages();

    public T[] LoadPackageItems<T>(
        string packageName,
        string itemType) =>
        packageProcessingService.LoadPackageItems<T>(
            packageName: packageName,
            itemType: itemType);

    public CommonObject[] LoadCommonObjects() =>
        commonObjectProcessingService.LoadCommonObjects();
}