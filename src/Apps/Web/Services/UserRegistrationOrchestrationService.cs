using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services.Orchestrations.Interfaces;
using cCoder.Security.Api.Interfaces;
using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class UserRegistrationOrchestrationService : IUserRegistrationOrchestrationService
    {
        private readonly IAccountManager accountManager;
        private readonly ICMSUserRegistrationOrchestrationService coreUserService;

        public UserRegistrationOrchestrationService(
            IAccountManager accountManager,
            ICMSUserRegistrationOrchestrationService coreUserService)
        {
            this.accountManager = accountManager;
            this.coreUserService = coreUserService;
        }

        public async ValueTask ConfirmRegistrationAsync(string token) =>
            await accountManager.ConfirmRegistrationAsync(token);


        public async ValueTask<Token> LoginAsync(string username, string password) =>
            await accountManager.LoginAsync(username, password);

        public async ValueTask LogoutAsync() =>
            await accountManager.LogoutAsync();

        public SSOUser Me() =>
            accountManager.Me();

        public async ValueTask<SSOUser> RegisterAsync(RegisterUser registerForm)
        {
            (SSOUser ssoUser, string confirmationToken) =
                await accountManager.RegisterAsync(registerForm);

            var coreUser = new User
            {
                DefaultCultureId = registerForm.Culture,
                Id = ssoUser.Id,
                Email = ssoUser.Email,
                DisplayName = ssoUser.DisplayName,
                IsActive = true
            };


            //TODO: get token from SSO and then when it comes back 
            await coreUserService.RegisterUserAsync(coreUser, registerForm.AppId, confirmationToken);
            return ssoUser;
        }
    }
}