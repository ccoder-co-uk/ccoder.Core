// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class ContentManagementPackageProcessingService
{
    private static void ValidatePackageOnImport(int? appId, Package package)
    {
        ValidationRulesEngine.Validate(inputs: [package]);

        if (appId.HasValue)
        {
            ValidationRulesEngine.Validate(inputs: [appId.Value]);
        }
    }

    private static void ValidatePackageOnExport(int appId, string packageName) =>
        ValidationRulesEngine.Validate(inputs: [appId, packageName]);
}