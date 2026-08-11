// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Aggregations.Packages;
using cCoder.Core.Services.Processings.Packages;
using Moq;

namespace cCoder.Core.Tests;

public sealed partial class PackageImportAggregationServiceTests
{
    private readonly Mock<ICorePackageProcessingService>
        corePackageProcessingServiceMock = new(MockBehavior.Strict);

    private readonly Mock<IContentManagementPackageProcessingService>
        contentManagementPackageProcessingServiceMock =
            new(MockBehavior.Strict);

    private readonly Mock<IAppSecurityPackageProcessingService>
        appSecurityPackageProcessingServiceMock =
            new(MockBehavior.Strict);

    private readonly Mock<IDocumentManagementPackageProcessingService>
        documentManagementPackageProcessingServiceMock =
            new(MockBehavior.Strict);

    private readonly Mock<IWorkflowPackageProcessingService>
        workflowPackageProcessingServiceMock =
            new(MockBehavior.Strict);

    private readonly Mock<IPackageImportCompletionEventProcessingService>
        packageImportCompletionEventProcessingServiceMock =
            new(MockBehavior.Strict);

    private PackageImportAggregationService CreateService() =>
        new(
            corePackageProcessingService:
                corePackageProcessingServiceMock.Object,
            contentManagementPackageProcessingService:
                contentManagementPackageProcessingServiceMock.Object,
            appSecurityPackageProcessingService:
                appSecurityPackageProcessingServiceMock.Object,
            documentManagementPackageProcessingService:
                documentManagementPackageProcessingServiceMock.Object,
            workflowPackageProcessingService:
                workflowPackageProcessingServiceMock.Object,
            packageImportCompletionEventProcessingService:
                packageImportCompletionEventProcessingServiceMock.Object);
}
