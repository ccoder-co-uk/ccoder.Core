// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Aggregations;
using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;

using cCoder.Core.Services.Orchestrations;

namespace cCoder.Core.Exposures.Managers;

internal sealed class UserRegistrationManager(
    IUserRegistrationAggregationService userRegistrationAggregationService)
    : IUserRegistrationOrchestrationService
{
    public ValueTask ConfirmRegistrationAsync(string token) =>
        userRegistrationAggregationService.ConfirmRegistrationAsync(
            token: token);

    public ValueTask<Token> LoginAsync(
        string username,
        string password) =>
        userRegistrationAggregationService.LoginAsync(
            username: username,
            password: password);

    public ValueTask LogoutAsync() =>
        userRegistrationAggregationService.LogoutAsync();

    public SSOUser Me() =>
        userRegistrationAggregationService.Me();

    public ValueTask<SSOUser> RegisterAsync(RegisterUser registerForm) =>
        userRegistrationAggregationService.RegisterUserAsync(
            registerUser: registerForm);
}