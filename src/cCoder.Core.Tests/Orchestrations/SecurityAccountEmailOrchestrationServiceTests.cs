// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Exposures.Managers;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Security.Models.Entities;
using cCoder.Security.Models.Events;
using FluentAssertions;
using Moq;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    private readonly Mock<IContentManagementAppService> contentManagementAppServiceMock;
    private readonly Mock<ITemplatedEmailManager> templatedEmailManagerMock;
    private readonly SecurityAccountEmailAggregationService orchestrationService;

    public SecurityAccountEmailOrchestrationServiceTests()
    {
        contentManagementAppServiceMock = new Mock<IContentManagementAppService>(MockBehavior.Strict);
        templatedEmailManagerMock =
            new Mock<ITemplatedEmailManager>(MockBehavior.Strict);

        orchestrationService = new SecurityAccountEmailAggregationService(
            contentManagementAppServiceMock.Object,
            templatedEmailManagerMock.Object);
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

    private void SetupAppLookup(App app) =>
        contentManagementAppServiceMock
            .Setup(expression: service => service.GetAllApps(ignoreFilters: true))
            .Returns(value: new[] { app }.AsQueryable());

    private void SetupQueuedEmailExpectation(
        string templateName,
        string subject) =>
        templatedEmailManagerMock
            .Setup(expression: service =>
                service.QueueAppTemplatedEmailAsync(
                app: It.Is<App>(match: app => app.Id == 17),
                templateName: templateName,
                culture: "cy-GB",
                model: It.Is<object>(match: model =>
                    ReadToken(model: model) == "token-123"
                    && ReadUser(model: model).Id == "user-123"),
                toEmail: "user@example.com",
                subject: subject,
                sentByUserId: "user-123",
                mailSenderName: "Default"))
            .ReturnsAsync(value: new QueuedEmail());

    private static string ReadToken(object model) =>
        (string)model.GetType()
            .GetProperty(name: "Token")!
            .GetValue(obj: model);

    private static SSOUser ReadUser(object model) =>
        (SSOUser)model.GetType()
            .GetProperty(name: "SSOUser")!
            .GetValue(obj: model);

    private void VerifyQueuedEmail(
        string templateName,
        string subject) =>
        templatedEmailManagerMock.Verify(
            expression: service =>
                service.QueueAppTemplatedEmailAsync(
                app: It.Is<App>(match: app => app.Id == 17),
                templateName: templateName,
                culture: "cy-GB",
                model: It.IsAny<object>(),
                toEmail: "user@example.com",
                subject: subject,
                sentByUserId: "user-123",
                mailSenderName: "Default"),
            times: Times.Once);
}