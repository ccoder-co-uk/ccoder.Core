// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Models.Events;
using Xunit;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    [Fact]
    public async Task QueueInvitationCreatedSecurityAccountEventEmailAsync_ShouldQueueInvitationEmail()
    {
        // Given
        App app = CreateApp(templateName: "UserInvite");
        SecurityAccountEvent accountEvent = CreateAccountEvent(kind: SecurityAccountEventKind.InvitationCreated);
        SetupAppLookup(app: app);

        SetupQueuedEmailExpectation(
            templateName: "UserInvite",
            subject: "Core Portal: Confirm Invitation");

        // When
        await orchestrationService.QueueInvitationCreatedSecurityAccountEventEmailAsync(accountEvent: accountEvent);

        // Then
        VerifyQueuedEmail(
            templateName: "UserInvite",
            subject: "Core Portal: Confirm Invitation");
    }
}