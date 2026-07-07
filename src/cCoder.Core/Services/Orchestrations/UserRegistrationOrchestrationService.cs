using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;
using cCoder.Security.Services.Orchestrations.Interfaces;

namespace cCoder.Core.Services.Orchestrations;

public class UserRegistrationOrchestrationService(
    IAuthenticationOrchestrationService authenticationOrchestrationService,
    ISSOUserOrchestrationService ssoUserOrchestrationService)
    : IUserRegistrationOrchestrationService
{
    public ValueTask ConfirmRegistrationAsync(string token) =>
        ssoUserOrchestrationService.ConfirmRegistration(token);

    public ValueTask<Token> LoginAsync(string username, string password) =>
        authenticationOrchestrationService.LoginAsync(username, password);

    public ValueTask LogoutAsync() =>
        authenticationOrchestrationService.LogoutAsync();

    public SSOUser Me() =>
        authenticationOrchestrationService.Me();

    public async ValueTask<SSOUser> RegisterAsync(RegisterUser registerForm) =>
        (await ssoUserOrchestrationService.Register(registerForm)).Item1;
}
