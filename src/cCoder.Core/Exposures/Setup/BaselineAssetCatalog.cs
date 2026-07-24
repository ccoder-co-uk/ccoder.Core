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
    private readonly IBaselineAssetCatalogProcessingService processingService;

    public BaselineAssetCatalog()
        : this(new BaselineAssetCatalogProcessingService())
    {
    }

    internal BaselineAssetCatalog(Assembly assembly)
        : this(new BaselineAssetCatalogProcessingService(assembly: assembly))
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
        processingService.LoadCoreReviewPackages();

    public Package[] LoadPackages() =>
        processingService.LoadPackages();

    public T[] LoadPackageItems<T>(
        string packageName,
        string itemType) =>
        processingService.LoadPackageItems<T>(
            packageName: packageName,
            itemType: itemType);

    public CommonObject[] LoadCommonObjects() =>
        processingService.LoadCommonObjects();
}