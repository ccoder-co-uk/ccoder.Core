// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;
using cCoder.Security.Services.Aggregations.Interfaces;

namespace cCoder.Core.Services.Orchestrations;

public class UserRegistrationOrchestrationService(
    IAuthenticationAggregationService authenticationAggregationService,
    IRegistrationAggregationService registrationAggregationService,
    ICurrentUserAggregationService currentUserAggregationService)
    : IUserRegistrationOrchestrationService
{
    public ValueTask ConfirmRegistrationAsync(string token) =>
        registrationAggregationService.ConfirmRegistration(tokenId: token);

    public ValueTask<Token> LoginAsync(string username, string password) =>
        authenticationAggregationService.LoginAsync(
            username: username,
            password: password);

    public ValueTask LogoutAsync() =>
        authenticationAggregationService.LogoutAsync();

    public SSOUser Me() =>
        currentUserAggregationService.GetCurrentUser();

    public async ValueTask<SSOUser> RegisterAsync(RegisterUser registerForm) =>
        (await registrationAggregationService.RegisterUserAsync(
            registerForm: registerForm)).User;
}