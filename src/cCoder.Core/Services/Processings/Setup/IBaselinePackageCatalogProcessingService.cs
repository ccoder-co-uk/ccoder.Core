// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Setup;

internal interface IBaselinePackageCatalogProcessingService
{
    Package[] LoadCoreReviewPackages();

    Package[] LoadPackages();

    T[] LoadPackageItems<T>(
        string packageName,
        string itemType);
}