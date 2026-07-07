using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Security.Objects.Entities;
using cCoder.Security.Objects.Events;
using FluentAssertions;
using Moq;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    private readonly Mock<IContentManagementAppService> contentManagementAppServiceMock;
    private readonly Mock<ITemplatedEmailOrchestrationService> templatedEmailOrchestrationServiceMock;
    private readonly SecurityAccountEmailOrchestrationService orchestrationService;

    public SecurityAccountEmailOrchestrationServiceTests()
    {
        contentManagementAppServiceMock = new Mock<IContentManagementAppService>(MockBehavior.Strict);
        templatedEmailOrchestrationServiceMock = new Mock<ITemplatedEmailOrchestrationService>(MockBehavior.Strict);
        orchestrationService = new SecurityAccountEmailOrchestrationService(
            contentManagementAppServiceMock.Object,
            templatedEmailOrchestrationServiceMock.Object);
    }

    private static App CreateApp(string templateName) =>
        new()
        {
            Id = 17,
            Name = "Core Portal",
            Domain = "example.com",
            DefaultCultureId = "en-GB",
            Templates =
            [
                new Template
                {
                    Name = templateName,
                }
            ]
        };

    private static SecurityAccountEvent CreateAccountEvent(SecurityAccountEventKind kind) =>
        new()
        {
            Kind = kind,
            RequestDomain = "https://example.com:7158",
            Token = "token-123",
            Culture = "cy-GB",
            User = new SSOUser
            {
                Id = "user-123",
                Email = "user@example.com",
                DisplayName = "Example User",
            }
        };

    private void SetupAppLookup(App app)
    {
        contentManagementAppServiceMock
            .Setup(service => service.GetAll(true))
            .Returns(new[] { app }.AsQueryable());
    }

    private void SetupQueuedEmailExpectation(string templateName, string subject)
    {
        templatedEmailOrchestrationServiceMock
            .Setup(service => service.QueueAsync(
                It.Is<App>(app => app.Id == 17),
                templateName,
                "cy-GB",
                It.Is<object>(model =>
                    ReadModelValue<string>(model, "Token") == "token-123"
                    && ReadModelValue<SSOUser>(model, "SSOUser").Id == "user-123"),
                "user@example.com",
                subject,
                "user-123",
                "Default"))
            .ReturnsAsync(new QueuedEmail());
    }

    private static TValue ReadModelValue<TValue>(object model, string propertyName) =>
        (TValue)model.GetType().GetProperty(propertyName)!.GetValue(model);

    private void VerifyQueuedEmail(string templateName, string subject) =>
        templatedEmailOrchestrationServiceMock.Verify(service => service.QueueAsync(
                It.Is<App>(app => app.Id == 17),
                templateName,
                "cy-GB",
                It.IsAny<object>(),
                "user@example.com",
                subject,
                "user-123",
                "Default"),
            Times.Once);
}
