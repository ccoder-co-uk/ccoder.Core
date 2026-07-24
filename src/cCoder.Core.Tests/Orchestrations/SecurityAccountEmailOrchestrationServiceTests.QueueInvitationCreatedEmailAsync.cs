// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.Events;
using Xunit;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    [Fact]
    public async Task QueueInvitationCreatedEmailAsync_ShouldQueueInvitationEmail()
    {
        App app = CreateApp("UserInvite");
        SecurityAccountEvent accountEvent = CreateAccountEvent(SecurityAccountEventKind.InvitationCreated);
        SetupAppLookup(app);
        SetupQueuedEmailExpectation(
            "UserInvite",
            "Core Portal: Confirm Invitation");

        await orchestrationService.QueueInvitationCreatedEmailAsync(accountEvent);

        VerifyQueuedEmail(
            "UserInvite",
            "Core Portal: Confirm Invitation");
    }
}