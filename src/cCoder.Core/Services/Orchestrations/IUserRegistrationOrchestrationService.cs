// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Models;
using cCoder.Security.Models.DTOs;
using cCoder.Security.Models.Entities;

namespace cCoder.Core.Services.Orchestrations;

internal interface IUserRegistrationOrchestrationService
{
    ValueTask ConfirmRegistrationAsync(string token);
    ValueTask<Token> LoginAsync(string username, string password);
    ValueTask LogoutAsync();
    SSOUser Me();
    ValueTask<SSOUser> RegisterAsync(RegisterUser registerForm);
}