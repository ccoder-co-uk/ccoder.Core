// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Models;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed class AppAggregationServiceTests
{
    [Fact]
    public async Task DeleteAppAsyncDeletesContentManagementBeforeAppSecurity()
    {
        // given
        const int appId = 42;
        Mock<IContentManagementAppService> contentManagementAppServiceMock = new(MockBehavior.Strict);
        Mock<IAppSecurityAppService> appSecurityAppServiceMock = new(MockBehavior.Strict);
        Mock<IPlanningAppService> planningAppServiceMock = new(MockBehavior.Strict);
        Mock<IDocumentManagementAppService> documentManagementAppServiceMock = new(MockBehavior.Strict);
        Mock<IWorkflowAppService> workflowAppServiceMock = new(MockBehavior.Strict);
        Mock<IMailAppService> mailAppServiceMock = new(MockBehavior.Strict);
        MockSequence sequence = new();
        CoreConfiguration configuration = new();

        planningAppServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.DeleteAppAsync(appId: appId))
            .Returns(value: ValueTask.CompletedTask);

        documentManagementAppServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.DeleteAppAsync(appId: appId))
            .Returns(value: ValueTask.CompletedTask);

        workflowAppServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.DeleteAppAsync(appId: appId))
            .Returns(value: ValueTask.CompletedTask);

        mailAppServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.DeleteAppAsync(appId: appId))
            .Returns(value: ValueTask.CompletedTask);

        contentManagementAppServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.DeleteAppAsync(appId: appId))
            .Returns(value: ValueTask.CompletedTask);

        appSecurityAppServiceMock
            .InSequence(sequence: sequence)
            .Setup(expression: service => service.DeleteAppAsync(appId: appId))
            .Returns(value: ValueTask.CompletedTask);

        AppAggregationService service = new(
            contentManagementAppServiceMock.Object,
            appSecurityAppServiceMock.Object,
            planningAppServiceMock.Object,
            documentManagementAppServiceMock.Object,
            workflowAppServiceMock.Object,
            mailAppServiceMock.Object,
            configuration);

        // when
        await service.DeleteAppAsync(appId: appId);

        // then
        contentManagementAppServiceMock.Verify(
            expression: service => service.DeleteAppAsync(appId: appId),
            times: Times.Once);

        appSecurityAppServiceMock.Verify(
            expression: service => service.DeleteAppAsync(appId: appId),
            times: Times.Once);
    }
}