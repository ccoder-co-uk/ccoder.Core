// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Processings.Setup;

internal sealed partial class BaselineAssetCatalogProcessingService
{
    private static void ValidateRelativePathOnLoad(string relativePath) =>
        ValidationRulesEngine.Validate(inputs: [relativePath]);
}