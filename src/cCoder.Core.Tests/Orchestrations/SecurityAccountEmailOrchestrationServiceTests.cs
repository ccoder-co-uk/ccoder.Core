// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
            .Setup(expression: service => service.GetAllApps(ignoreFilters: true))
            .Returns(value: new[] { app }.AsQueryable());
    }

    private void SetupQueuedEmailExpectation(string templateName, string subject)
    {
        templatedEmailOrchestrationServiceMock
            .Setup(expression: service => service.QueueAsync(
app:                 It.Is<App>(match: app => app.Id == 17),templateName:                 templateName,culture:                 "cy-GB",model:                 It.Is<object>(match: model =>
                    ReadModelValue<string>(model: model,propertyName: "Token") == "token-123"
                    && ReadModelValue<SSOUser>(model: model,propertyName: "SSOUser").Id == "user-123"),toEmail:                 "user@example.com",subject:                 subject,sentByUserId:                 "user-123",mailSenderName:                 "Default"))
            .ReturnsAsync(value: new QueuedEmail());
    }

    private static TValue ReadModelValue<TValue>(object model, string propertyName) =>
        (TValue)model.GetType()
            .GetProperty(name: propertyName)!.GetValue(obj: model);

    private void VerifyQueuedEmail(string templateName, string subject) =>
        templatedEmailOrchestrationServiceMock.Verify(expression: service => service.QueueAsync(
app:                 It.Is<App>(match: app => app.Id == 17),templateName:                 templateName,culture:                 "cy-GB",model:                 It.IsAny<object>(),toEmail:                 "user@example.com",subject:                 subject,sentByUserId:                 "user-123",mailSenderName:                 "Default"),times:             Times.Once);
}