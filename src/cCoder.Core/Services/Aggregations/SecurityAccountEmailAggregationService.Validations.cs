// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using cCoder.Core.Models.Exceptions;
using cCoder.Security.Objects.Events;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class SecurityAccountEmailAggregationService
{
    private static void ValidateSecurityAccountEventOnQueue(
        SecurityAccountEvent accountEvent)
    {
        if (string.IsNullOrWhiteSpace(value: accountEvent?.RequestDomain))
        {
            return;
        }

        if (accountEvent.User is null)
        {
            ValidationException validationException = new(
                message: "Security account event user is required.");

            throw new CoreValidationException(
                innerException: validationException);
        }

        if (string.IsNullOrWhiteSpace(value: accountEvent.User.Email))
        {
            ValidationException validationException = new(
                message: "Security account event user email is required.");

            throw new CoreValidationException(
                innerException: validationException);
        }
    }
}