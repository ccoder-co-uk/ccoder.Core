// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.Events;
using Xunit;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    [Fact]
    public async Task QueueRegistrationCreatedSecurityAccountEventEmailAsync_ShouldQueueConfirmRegistrationEmail()
    {
        App app = CreateApp(templateName: "ConfirmRegistration");
        SecurityAccountEvent accountEvent = CreateAccountEvent(kind: SecurityAccountEventKind.RegistrationCreated);
        SetupAppLookup(app: app);

        SetupQueuedEmailExpectation(
templateName:             "ConfirmRegistration",subject:             "Core Portal: Confirm Registration");

        await orchestrationService.QueueRegistrationCreatedSecurityAccountEventEmailAsync(accountEvent: accountEvent);

        VerifyQueuedEmail(
templateName:             "ConfirmRegistration",subject:             "Core Portal: Confirm Registration");
    }
}