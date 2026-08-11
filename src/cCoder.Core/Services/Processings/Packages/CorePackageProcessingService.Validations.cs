// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class CorePackageProcessingService
{
    private static void ValidatePackageOnImport(
        int appId,
        Package package) =>
        ValidationRulesEngine.Validate(inputs: [appId, package]);
}