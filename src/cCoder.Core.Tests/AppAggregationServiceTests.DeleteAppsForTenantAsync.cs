// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;
using cCoder.Core.Models;
using cCoder.Data.Models.CMS;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class AppAggregationServiceTests
{
    [Fact]
    public async Task DeleteAppsForTenantAsyncDeletesOnlyTheTenantApps()
    {
        // Given
        const string tenantId = "tenant-1";
        const int ownedAppId = 42;

        App[] apps =
        [
            new() { Id = ownedAppId, TenantId = tenantId },
            new() { Id = 99, TenantId = "other-tenant" }
        ];

        Mock<IContentManagementAppService> contentManagementAppServiceMock = new(MockBehavior.Strict);
        Mock<IAppSecurityAppService> appSecurityAppServiceMock = new(MockBehavior.Strict);
        Mock<IPlanningAppService> planningAppServiceMock = new(MockBehavior.Strict);
        Mock<IDocumentManagementAppService> documentManagementAppServiceMock = new(MockBehavior.Strict);
        Mock<IWorkflowAppService> workflowAppServiceMock = new(MockBehavior.Strict);
        Mock<IMailAppService> mailAppServiceMock = new(MockBehavior.Strict);
        Mock<IAppGraphEventService> appGraphEventServiceMock = new(MockBehavior.Strict);

        contentManagementAppServiceMock
            .Setup(expression: service => service.GetAllApps(ignoreFilters: true))
            .Returns(value: apps.AsQueryable());

        planningAppServiceMock
            .Setup(expression: service => service.DeleteAppAsync(appId: ownedAppId))
            .Returns(value: ValueTask.CompletedTask);

        documentManagementAppServiceMock
            .Setup(expression: service => service.DeleteAppAsync(appId: ownedAppId))
            .Returns(value: ValueTask.CompletedTask);

        workflowAppServiceMock
            .Setup(expression: service => service.DeleteAppAsync(appId: ownedAppId))
            .Returns(value: ValueTask.CompletedTask);

        mailAppServiceMock
            .Setup(expression: service => service.DeleteAppAsync(appId: ownedAppId))
            .Returns(value: ValueTask.CompletedTask);

        contentManagementAppServiceMock
            .Setup(expression: service => service.DeleteAppAsync(appId: ownedAppId))
            .Returns(value: ValueTask.CompletedTask);

        appSecurityAppServiceMock
            .Setup(expression: service => service.DeleteAppAsync(appId: ownedAppId))
            .Returns(value: ValueTask.CompletedTask);

        AppAggregationService service = new(
            contentManagementAppService: contentManagementAppServiceMock.Object,
            appSecurityAppService: appSecurityAppServiceMock.Object,
            planningAppService: planningAppServiceMock.Object,
            documentManagementAppService: documentManagementAppServiceMock.Object,
            workflowAppService: workflowAppServiceMock.Object,
            mailAppService: mailAppServiceMock.Object,
            appGraphEventService: appGraphEventServiceMock.Object,
            configuration: CoreConfigurationFactory.Create());

        // When
        await service.DeleteAppsForTenantAsync(tenantId: tenantId);

        // Then
        planningAppServiceMock.Verify(
            expression: dependency => dependency.DeleteAppAsync(appId: ownedAppId),
            times: Times.Once);

        documentManagementAppServiceMock.Verify(
            expression: dependency => dependency.DeleteAppAsync(appId: ownedAppId),
            times: Times.Once);

        workflowAppServiceMock.Verify(
            expression: dependency => dependency.DeleteAppAsync(appId: ownedAppId),
            times: Times.Once);

        mailAppServiceMock.Verify(
            expression: dependency => dependency.DeleteAppAsync(appId: ownedAppId),
            times: Times.Once);

        contentManagementAppServiceMock.Verify(
            expression: dependency => dependency.DeleteAppAsync(appId: ownedAppId),
            times: Times.Once);

        appSecurityAppServiceMock.Verify(
            expression: dependency => dependency.DeleteAppAsync(appId: ownedAppId),
            times: Times.Once);

        planningAppServiceMock.Verify(
            expression: dependency => dependency.DeleteAppAsync(appId: 99),
            times: Times.Never);
    }
}
