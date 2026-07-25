// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;

namespace cCoder.Core.Services.Aggregations;

internal interface IUserRegistrationAggregationService
{
    ValueTask<UserRegistrationOperation>
        ExecuteUserRegistrationOperationAsync(
            UserRegistrationOperation userRegistrationOperation);

    UserRegistrationOperation GetUserRegistrationOperation(
        UserRegistrationOperation userRegistrationOperation);
}