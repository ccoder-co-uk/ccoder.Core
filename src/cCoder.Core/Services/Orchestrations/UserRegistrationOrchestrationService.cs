using cCoder.AppSecurity.Models;
using cCoder.Data.Models.Security;
using cCoder.Security.Exposures;
using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;

namespace cCoder.Core.Services.Orchestrations;

public class UserRegistrationOrchestrationService(
    IAccountManager accountManager,
    ICMSUserRegistrationOrchestrationService coreUserProcessingService)
    : IUserRegistrationOrchestrationService
{
    public ValueTask ConfirmRegistrationAsync(string token) =>
        accountManager.ConfirmRegistrationAsync(token);

    public ValueTask<Token> LoginAsync(string username, string password) =>
        accountManager.LoginAsync(username, password);

    public ValueTask LogoutAsync() =>
        accountManager.LogoutAsync();

    public SSOUser Me() =>
        accountManager.Me();

    public async ValueTask<SSOUser> RegisterAsync(RegisterUser registerForm)
    {
        (SSOUser ssoUser, string confirmationToken) =
            await accountManager.RegisterAsync(registerForm);

        User coreUser = new()
        {
            DefaultCultureId = registerForm.Culture,
            Id = ssoUser.Id,
            Email = ssoUser.Email,
            DisplayName = ssoUser.DisplayName,
            IsActive = true,
        };

        await coreUserProcessingService.RegisterUserAsync(
            coreUser,
            registerForm.AppId,
            confirmationToken);

        return ssoUser;
    }
}

