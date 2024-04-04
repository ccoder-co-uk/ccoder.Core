using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services.Orchestrations.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Security.Objects.DTOs;

namespace Web.Api.Controllers
{
    [Route("Api/Account")]
    public class AccountController : Controller
    {
        private readonly ICMSUserRegistrationOrchestrationService userRegistrationOrchestrationService;

        public AccountController(ICMSUserRegistrationOrchestrationService userRegistrationOrchestrationService) =>
            this.userRegistrationOrchestrationService = userRegistrationOrchestrationService;

        [HttpPost("Login")]
        public IActionResult Login([FromBody] Auth auth)
        {
            Program.SSOUserId = auth.User;
            return Ok();
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Program.SSOUserId = "Guest";
            return Ok();
        }

        [HttpPost("ForgotPassword")]
        public IActionResult ChangePassword(string email, int appId) =>
             Ok();

        [HttpPost("Register")]
        public async ValueTask<IActionResult> Register([FromBody] RegisterUser registerForm)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var coreUser = new User
            {
                DefaultCultureId = registerForm.Culture,
                Id = registerForm.Email.Split("@")[0],
                Email = registerForm.Email,
                DisplayName = registerForm.DisplayName,
                IsActive = true
            };

            var newUser = await userRegistrationOrchestrationService.RegisterUserAsync(
                coreUser,
                registerForm.AppId,
                Guid.NewGuid().ToString());

            return Ok(newUser);
        }

        [HttpPost("ConfirmRegistration")]
        public IActionResult ConfirmRegistration(string confirmationToken) =>
            Ok();
    }
}