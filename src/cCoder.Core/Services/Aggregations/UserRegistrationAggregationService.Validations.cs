// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Security.Objects.DTOs;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class UserRegistrationAggregationService
{
    private static void ValidateTokenOnConfirmRegistration(string token) =>
        ValidationRulesEngine.Validate(inputs: [token]);

    private static void ValidateCredentialsOnLogin(
        string username,
        string password) =>
        ValidationRulesEngine.Validate(inputs: [username, password]);

    private static void ValidateRegisterUserOnRegister(
        RegisterUser registerUser) =>
        ValidationRulesEngine.Validate(inputs: [registerUser]);
}