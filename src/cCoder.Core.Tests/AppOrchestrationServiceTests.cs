using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;
using cCoder.Core.Services.Orchestrations;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed class AppOrchestrationServiceTests
{
    [Fact]
    public async Task DeleteAsync_ShouldDeleteContentManagementBeforeAppSecurity()
    {
        const int appId = 42;
        Mock<IContentManagementAppService> contentManagementAppServiceMock = new(MockBehavior.Strict);
        Mock<IAppSecurityAppService> appSecurityAppServiceMock = new(MockBehavior.Strict);
        Mock<IPlanningAppService> planningAppServiceMock = new(MockBehavior.Strict);
        Mock<IDocumentManagementAppService> documentManagementAppServiceMock = new(MockBehavior.Strict);
        Mock<IWorkflowAppService> workflowAppServiceMock = new(MockBehavior.Strict);
        Mock<IMailAppService> mailAppServiceMock = new(MockBehavior.Strict);
        MockSequence sequence = new();

        planningAppServiceMock
            .InSequence(sequence)
            .Setup(service => service.DeleteAsync(appId))
            .Returns(ValueTask.CompletedTask);
        documentManagementAppServiceMock
            .InSequence(sequence)
            .Setup(service => service.DeleteAsync(appId))
            .Returns(ValueTask.CompletedTask);
        workflowAppServiceMock
            .InSequence(sequence)
            .Setup(service => service.DeleteAsync(appId))
            .Returns(ValueTask.CompletedTask);
        mailAppServiceMock
            .InSequence(sequence)
            .Setup(service => service.DeleteAsync(appId))
            .Returns(ValueTask.CompletedTask);
        contentManagementAppServiceMock
            .InSequence(sequence)
            .Setup(service => service.DeleteAsync(appId))
            .Returns(ValueTask.CompletedTask);
        appSecurityAppServiceMock
            .InSequence(sequence)
            .Setup(service => service.DeleteAsync(appId))
            .Returns(ValueTask.CompletedTask);

        AppOrchestrationService service = new(
            contentManagementAppServiceMock.Object,
            appSecurityAppServiceMock.Object,
            planningAppServiceMock.Object,
            documentManagementAppServiceMock.Object,
            workflowAppServiceMock.Object,
            mailAppServiceMock.Object);

        await service.DeleteAsync(appId);

        contentManagementAppServiceMock.Verify(service => service.DeleteAsync(appId), Times.Once);
        appSecurityAppServiceMock.Verify(service => service.DeleteAsync(appId), Times.Once);
    }
}
