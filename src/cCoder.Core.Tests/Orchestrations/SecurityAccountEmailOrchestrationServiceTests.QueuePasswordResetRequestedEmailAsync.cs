// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.Events;
using Xunit;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    [Fact]
    public async Task QueuePasswordResetRequestedEmailAsync_ShouldQueueForgotPasswordEmail()
    {
        App app = CreateApp("ForgotPassword");
        SecurityAccountEvent accountEvent = CreateAccountEvent(SecurityAccountEventKind.PasswordResetRequested);
        SetupAppLookup(app);
        SetupQueuedEmailExpectation(
            "ForgotPassword",
            "Core Portal: Password Reset");

        await orchestrationService.QueuePasswordResetRequestedEmailAsync(accountEvent);

        VerifyQueuedEmail(
            "ForgotPassword",
            "Core Portal: Password Reset");
    }
}