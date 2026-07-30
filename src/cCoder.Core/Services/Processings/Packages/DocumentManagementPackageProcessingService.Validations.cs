// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class DocumentManagementPackageProcessingService
{
    private static void ValidatePackageOnImport(int appId, Package package) =>
        ValidationRulesEngine.Validate(inputs: [appId, package]);

    private static void ValidatePackageOnExport(int appId, string packageName) =>
        ValidationRulesEngine.Validate(inputs: [appId, packageName]);
}