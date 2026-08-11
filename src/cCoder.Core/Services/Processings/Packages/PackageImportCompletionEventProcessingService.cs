// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Packaging.Models;
using ContentManagementPackageImportEvent = cCoder.ContentManagement.Models.PackageImportEvent;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class PackageImportCompletionEventProcessingService(
    IPackageImportCompletionEventService packageImportCompletionEventService)
    : IPackageImportCompletionEventProcessingService
{
    public ValueTask ProcessPackageImportEventAsync(
        PackageImportEvent packageImportEvent) =>
        TryCatch(operation: async () =>
        {
            ValidatePackageImportEvent(packageImportEvent: packageImportEvent);

            await packageImportCompletionEventService
                .RaisePackageImportEventCompleteAsync(
                    packageImportEvent: new ContentManagementPackageImportEvent
                    {
                        AppId = packageImportEvent.AppId,
                        Package = packageImportEvent.Package,
                    });
        });
}