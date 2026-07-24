// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.Events;
using Xunit;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    [Fact]
    public async Task QueuePasswordResetRequestedSecurityAccountEventEmailAsync_ShouldQueueForgotPasswordEmail()
    {
        App app = CreateApp(templateName: "ForgotPassword");
        SecurityAccountEvent accountEvent = CreateAccountEvent(kind: SecurityAccountEventKind.PasswordResetRequested);
        SetupAppLookup(app: app);

        SetupQueuedEmailExpectation(
templateName:             "ForgotPassword",subject:             "Core Portal: Password Reset");

        await orchestrationService.QueuePasswordResetRequestedSecurityAccountEventEmailAsync(accountEvent: accountEvent);

        VerifyQueuedEmail(
templateName:             "ForgotPassword",subject:             "Core Portal: Password Reset");
    }
}