// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Services.Orchestrations;
using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;

namespace cCoder.Core.Exposures.Managers;

internal sealed class UserRegistrationManager(
    IUserRegistrationAggregationService
        userRegistrationAggregationService
) : IUserRegistrationOrchestrationService
{
    public async ValueTask ConfirmRegistrationAsync(string token)
    {
        UserRegistrationOperation userRegistrationOperation = new()
        {
            Type =
                UserRegistrationOperationType.ConfirmRegistration,
            RegistrationToken = token,
        };

        await userRegistrationAggregationService
            .ExecuteUserRegistrationOperationAsync(
                userRegistrationOperation:
                    userRegistrationOperation);
    }

    public async ValueTask<Token> LoginAsync(
        string username,
        string password)
    {
        UserRegistrationOperation userRegistrationOperation = new()
        {
            Type = UserRegistrationOperationType.Login,
            Username = username,
            Password = password,
        };

        UserRegistrationOperation completedOperation =
            await userRegistrationAggregationService
                .ExecuteUserRegistrationOperationAsync(
                    userRegistrationOperation:
                        userRegistrationOperation);

        return completedOperation.AuthenticationToken;
    }

    public async ValueTask LogoutAsync()
    {
        UserRegistrationOperation userRegistrationOperation = new()
        {
            Type = UserRegistrationOperationType.Logout,
        };

        await userRegistrationAggregationService
            .ExecuteUserRegistrationOperationAsync(
                userRegistrationOperation:
                    userRegistrationOperation);
    }

    public SSOUser Me()
    {
        UserRegistrationOperation userRegistrationOperation = new()
        {
            Type = UserRegistrationOperationType.GetCurrentUser,
        };

        UserRegistrationOperation completedOperation =
            userRegistrationAggregationService
                .GetUserRegistrationOperation(
                    userRegistrationOperation:
                        userRegistrationOperation);

        return completedOperation.User;
    }

    public async ValueTask<SSOUser> RegisterAsync(
        RegisterUser registerForm)
    {
        UserRegistrationOperation userRegistrationOperation = new()
        {
            Type = UserRegistrationOperationType.RegisterUser,
            Registration = registerForm,
        };

        UserRegistrationOperation completedOperation =
            await userRegistrationAggregationService
                .ExecuteUserRegistrationOperationAsync(
                    userRegistrationOperation:
                        userRegistrationOperation);

        return completedOperation.User;
    }
}