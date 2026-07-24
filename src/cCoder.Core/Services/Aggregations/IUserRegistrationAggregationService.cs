// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;

namespace cCoder.Core.Services.Aggregations;

internal interface IUserRegistrationAggregationService
{
    ValueTask ConfirmRegistrationAsync(string token);

    ValueTask<Token> LoginAsync(string username, string password);

    ValueTask LogoutAsync();

    SSOUser Me();

    ValueTask<SSOUser> RegisterUserAsync(RegisterUser registerUser);
}