// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Foundations.Packages;

internal sealed partial class CorePackageService
{
    private static void ValidatePackageOperation(int appId, string value) =>
        ValidationRulesEngine.Validate(inputs: [appId]);
}