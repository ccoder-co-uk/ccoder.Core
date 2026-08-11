// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Packages;
using cCoder.Packaging.Models;

namespace cCoder.Core.Services.Aggregations.Packages;

internal sealed partial class PackageImportAggregationService(
    ICorePackageProcessingService corePackageProcessingService,
    IContentManagementPackageProcessingService contentManagementPackageProcessingService,
    IAppSecurityPackageProcessingService appSecurityPackageProcessingService,
    IDocumentManagementPackageProcessingService documentManagementPackageProcessingService,
    IWorkflowPackageProcessingService workflowPackageProcessingService,
    IPackageImportCompletionEventProcessingService packageImportCompletionEventProcessingService)
    : IPackageImportAggregationService
{
    public ValueTask ProcessPackageImportEventAsync(
        PackageImportEvent packageImportEvent) =>
        TryCatch(operation: async () =>
        {
            ValidatePackageImportEvent(packageImportEvent: packageImportEvent);

            if (packageImportEvent.AppId is not int appId)
            {
                await contentManagementPackageProcessingService.ImportPackageAsync(
                    appId: null,
                    package: packageImportEvent.Package);

                await packageImportCompletionEventProcessingService
                    .ProcessPackageImportEventAsync(
                        packageImportEvent: packageImportEvent);

                return;
            }

            await corePackageProcessingService.ImportPackageAsync(
                appId: appId,
                package: packageImportEvent.Package);

            await contentManagementPackageProcessingService.ImportPackageAsync(
                appId: appId,
                package: packageImportEvent.Package);

            await appSecurityPackageProcessingService.ImportPackageAsync(
                appId: appId,
                package: packageImportEvent.Package);

            await documentManagementPackageProcessingService.ImportPackageAsync(
                appId: appId,
                package: packageImportEvent.Package);

            await workflowPackageProcessingService.ImportPackageAsync(
                appId: appId,
                package: packageImportEvent.Package);

            await packageImportCompletionEventProcessingService
                .ProcessPackageImportEventAsync(
                    packageImportEvent: packageImportEvent);
        });
}