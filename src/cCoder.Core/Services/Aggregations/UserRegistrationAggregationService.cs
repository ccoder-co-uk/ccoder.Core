// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Security.Services.Aggregations.Interfaces;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class UserRegistrationAggregationService(
    IAuthenticationAggregationService authenticationAggregationService,
    IRegistrationAggregationService registrationAggregationService,
    ICurrentUserAggregationService currentUserAggregationService
) : IUserRegistrationAggregationService
{
    public ValueTask<UserRegistrationOperation>
        ExecuteUserRegistrationOperationAsync(
            UserRegistrationOperation userRegistrationOperation) =>
        TryCatch(operation: async () =>
        {
            ValidateUserRegistrationOperationOnExecute(
                userRegistrationOperation: userRegistrationOperation);

            switch (userRegistrationOperation.Type)
            {
                case UserRegistrationOperationType.ConfirmRegistration:
                    await registrationAggregationService
                        .ConfirmRegistration(
                            tokenId:
                                userRegistrationOperation
                                    .RegistrationToken);
                    break;

                case UserRegistrationOperationType.Login:
                    userRegistrationOperation.AuthenticationToken =
                        await authenticationAggregationService.LoginAsync(
                            username:
                                userRegistrationOperation.Username,
                            password:
                                userRegistrationOperation.Password);
                    break;

                case UserRegistrationOperationType.Logout:
                    await authenticationAggregationService.LogoutAsync();
                    break;

                case UserRegistrationOperationType.RegisterUser:
                    userRegistrationOperation.User =
                        (await registrationAggregationService
                            .RegisterUserAsync(
                                registerForm:
                                    userRegistrationOperation
                                        .Registration))
                        .User;
                    break;

                default:
                    throw new InvalidOperationException(
                        "The user registration operation is not asynchronous.");
            }

            return userRegistrationOperation;
        });

    public UserRegistrationOperation GetUserRegistrationOperation(
        UserRegistrationOperation userRegistrationOperation) =>
        TryCatch(operation: () =>
        {
            ValidateUserRegistrationOperationOnGet(
                userRegistrationOperation: userRegistrationOperation);

            userRegistrationOperation.User =
                currentUserAggregationService.GetCurrentUser();

            return userRegistrationOperation;
        });
}