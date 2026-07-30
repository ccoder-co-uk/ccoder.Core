// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Models.Events;
using Xunit;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    [Fact]
    public async Task QueuePasswordResetRequestedSecurityAccountEventEmailAsync_ShouldQueueForgotPasswordEmail()
    {
        // Given
        App app = CreateApp(templateName: "ForgotPassword");
        SecurityAccountEvent accountEvent = CreateAccountEvent(kind: SecurityAccountEventKind.PasswordResetRequested);
        SetupAppLookup(app: app);

        SetupQueuedEmailExpectation(
            templateName: "ForgotPassword",
            subject: "Core Portal: Password Reset");

        // When
        await orchestrationService.QueuePasswordResetRequestedSecurityAccountEventEmailAsync(accountEvent: accountEvent);

        // Then
        VerifyQueuedEmail(
            templateName: "ForgotPassword",
            subject: "Core Portal: Password Reset");
    }
}