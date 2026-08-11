// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.ContentManagement.Models;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class PackageImportCompletionEventService
{
    private static void ValidatePackageImportEventOnRaise(
        PackageImportEvent packageImportEvent) =>
        ValidationRulesEngine.Validate(
            inputs: [packageImportEvent, packageImportEvent?.Package]);
}