using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;

namespace Web.Services.Interfaces
{
    public interface IUserRegistrationOrchestrationService
    {
        ValueTask ConfirmRegistrationAsync(string token);
        ValueTask<Token> LoginAsync(string username, string password);
        ValueTask LogoutAsync();
        SSOUser Me();
        ValueTask<SSOUser> RegisterAsync(RegisterUser registerForm);
    }
}