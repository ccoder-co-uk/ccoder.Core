// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;
using cCoder.Security.Services.Aggregations.Interfaces;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class UserRegistrationAggregationService(
    IAuthenticationAggregationService authenticationAggregationService,
    IRegistrationAggregationService registrationAggregationService,
    ICurrentUserAggregationService currentUserAggregationService)
    : IUserRegistrationAggregationService
{
    public ValueTask ConfirmRegistrationAsync(string token) =>
        TryCatch(operation: async () =>
        {
            ValidateTokenOnConfirmRegistration(token: token);

            await registrationAggregationService.ConfirmRegistration(
                tokenId: token);
        });

    public ValueTask<Token> LoginAsync(
        string username,
        string password) =>
        TryCatch(operation: async () =>
        {
            ValidateCredentialsOnLogin(
                username: username,
                password: password);

            return await authenticationAggregationService.LoginAsync(
                username: username,
                password: password);
        });

    public ValueTask LogoutAsync() =>
        TryCatch(operation: async () =>
            await authenticationAggregationService.LogoutAsync());

    public SSOUser Me() =>
        TryCatch(operation: () =>
            currentUserAggregationService.GetCurrentUser());

    public ValueTask<SSOUser> RegisterUserAsync(RegisterUser registerUser) =>
        TryCatch(operation: async () =>
        {
            ValidateRegisterUserOnRegister(registerUser: registerUser);

            return (await registrationAggregationService.RegisterUserAsync(
                registerForm: registerUser)).User;
        });
}