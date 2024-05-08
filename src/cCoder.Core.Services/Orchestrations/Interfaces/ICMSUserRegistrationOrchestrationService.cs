using cCoder.Core.Objects.Entities.Security;

namespace cCoder.Core.Services.Orchestrations.Interfaces;

public interface ICMSUserRegistrationOrchestrationService
{
    ValueTask<User> RegisterUserAsync(User user, int appId, string confirmationToken);
}