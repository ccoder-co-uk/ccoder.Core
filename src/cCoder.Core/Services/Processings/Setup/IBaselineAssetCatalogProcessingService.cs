// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Setup;

internal interface IBaselineAssetCatalogProcessingService
{
    string LoadDefaultAppConfig();

    byte[] LoadAssetBytes(string relativePath);

    string[] LoadDmsAssetPaths();

    Package[] LoadCoreReviewPackages();

    Package[] LoadPackages();

    T[] LoadPackageItems<T>(string packageName, string itemType);

    CommonObject[] LoadCommonObjects();
}