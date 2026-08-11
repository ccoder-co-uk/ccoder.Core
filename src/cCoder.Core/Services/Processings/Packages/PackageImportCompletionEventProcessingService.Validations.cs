// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Packaging.Models;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class PackageImportCompletionEventProcessingService
{
    private static void ValidatePackageImportEvent(
        PackageImportEvent packageImportEvent) =>
        ValidationRulesEngine.Validate(
            inputs: [packageImportEvent, packageImportEvent?.Package]);
}