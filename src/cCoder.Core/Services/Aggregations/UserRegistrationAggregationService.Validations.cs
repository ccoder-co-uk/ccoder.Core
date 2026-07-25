// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class UserRegistrationAggregationService
{
    private static void ValidateUserRegistrationOperationOnExecute(
        UserRegistrationOperation userRegistrationOperation)
    {
        ValidationRulesEngine.Validate(
            inputs: [userRegistrationOperation]);

        object[] operationInputs =
            userRegistrationOperation.Type switch
            {
                UserRegistrationOperationType.ConfirmRegistration =>
                    [userRegistrationOperation.RegistrationToken],
                UserRegistrationOperationType.Login =>
                    [
                        userRegistrationOperation.Username,
                        userRegistrationOperation.Password,
                    ],
                UserRegistrationOperationType.RegisterUser =>
                    [userRegistrationOperation.Registration],
                _ => [],
            };

        ValidationRulesEngine.Validate(inputs: operationInputs);
    }

    private static void ValidateUserRegistrationOperationOnGet(
        UserRegistrationOperation userRegistrationOperation) =>
        ValidationRulesEngine.Validate(
            inputs: [userRegistrationOperation]);
}