// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Processings.Setup;

internal sealed partial class BaselinePackageCatalogProcessingService
{
    private static void ValidatePackageItemsOnLoad(
        string packageName,
        string itemType) =>
        ValidationRulesEngine.Validate(inputs: [packageName, itemType]);
}