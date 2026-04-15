using cCoder.AppSecurity.Models;
using cCoder.Security.Objects.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Security.Api.Controllers;

[Route("Api/Account")]
public class AccountController(
    cCoder.Core.Services.Orchestrations.IUserRegistrationOrchestrationService userRegistrationOrchestrationService,
    cCoder.Core.Services.Orchestrations.IUserPasswordOrchestrationService userPasswordOrchestrationService)
    : Controller
{
    [HttpPost("Login")]
    public async ValueTask<IActionResult> Login([FromBody] Auth auth) =>
        ModelState.IsValid
            ? Ok(await userRegistrationOrchestrationService.LoginAsync(auth.User, auth.Pass))
            : BadRequest(ModelState);

    [HttpPost("Logout")]
    public async ValueTask<IActionResult> Logout()
    {
        await userRegistrationOrchestrationService.LogoutAsync();
        return Ok();
    }

    [HttpPost("ForgotPassword")]
    public async ValueTask<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await userPasswordOrchestrationService.ForgotPasswordAsync(request.Email, request.AppId);
        return Ok();
    }

    [HttpPost("ConfirmForgotPassword")]
    public async ValueTask<IActionResult> ConfirmForgotPassword(
        [FromBody] ConfirmForgotPasswordRequest request)
    {
        await userPasswordOrchestrationService.ConfirmForgotPasswordAsync(
            request.Token,
            request.UserId,
            request.NewPassword,
            request.ConfirmPassword);
        return Ok();
    }

    [HttpPost("Register")]
    public async ValueTask<IActionResult> Register([FromBody] RegisterUser registerForm) =>
        ModelState.IsValid
            ? Ok(await userRegistrationOrchestrationService.RegisterAsync(registerForm))
            : BadRequest(ModelState);

    [HttpPost("ConfirmRegistration")]
    public async ValueTask<IActionResult> ConfirmRegistration(string confirmationToken)
    {
        await userRegistrationOrchestrationService.ConfirmRegistrationAsync(confirmationToken);
        return Ok();
    }
}
