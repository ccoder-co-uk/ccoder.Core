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
        App app = CreateApp(templateName: "UserInvite");
        SecurityAccountEvent accountEvent = CreateAccountEvent(kind: SecurityAccountEventKind.InvitationCreated);
        SetupAppLookup(app: app);

        SetupQueuedEmailExpectation(
templateName:             "UserInvite",subject:             "Core Portal: Confirm Invitation");

        await orchestrationService.QueueInvitationCreatedEmailAsync(accountEvent: accountEvent);

        VerifyQueuedEmail(
templateName:             "UserInvite",subject:             "Core Portal: Confirm Invitation");
    }
}