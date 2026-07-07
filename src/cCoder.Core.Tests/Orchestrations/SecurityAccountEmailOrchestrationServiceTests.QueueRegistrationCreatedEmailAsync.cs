using cCoder.Security.Objects.Events;
using Xunit;

namespace cCoder.Core.Tests.Orchestrations;

public partial class SecurityAccountEmailOrchestrationServiceTests
{
    [Fact]
    public async Task QueueRegistrationCreatedEmailAsync_ShouldQueueConfirmRegistrationEmail()
    {
        App app = CreateApp("ConfirmRegistration");
        SecurityAccountEvent accountEvent = CreateAccountEvent(SecurityAccountEventKind.RegistrationCreated);
        SetupAppLookup(app);
        SetupQueuedEmailExpectation(
            "ConfirmRegistration",
            "Core Portal: Confirm Registration");

        await orchestrationService.QueueRegistrationCreatedEmailAsync(accountEvent);

        VerifyQueuedEmail(
            "ConfirmRegistration",
            "Core Portal: Confirm Registration");
    }
}
