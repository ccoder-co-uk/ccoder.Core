// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Foundations.Packages;

internal sealed partial class DocumentManagementPackageService
{
    private static void ValidatePackageOnImport(int appId, Package package) =>
        ValidationRulesEngine.Validate(inputs: [appId, package]);

    private static void ValidatePackageOnExport(int appId, string packageName) =>
        ValidationRulesEngine.Validate(inputs: [appId, packageName]);
}