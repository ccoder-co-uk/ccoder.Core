// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class UserRegistrationAggregationService
{
    private static async ValueTask TryCatch(Func<ValueTask> operation)
    {
        try
        {
            await operation();
        }
        catch (CoreValidationException innerException)
        {
            throw new CoreOrchestrationValidationException(
                innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreOrchestrationDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreOrchestrationServiceException(
                innerException: innerException);
        }
    }

    private static async ValueTask<TResult> TryCatch<TResult>(
        Func<ValueTask<TResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (CoreValidationException innerException)
        {
            throw new CoreOrchestrationValidationException(
                innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreOrchestrationDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreOrchestrationServiceException(
                innerException: innerException);
        }
    }

    private static TResult TryCatch<TResult>(Func<TResult> operation)
    {
        try
        {
            return operation();
        }
        catch (CoreValidationException innerException)
        {
            throw new CoreOrchestrationValidationException(
                innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreOrchestrationDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreOrchestrationServiceException(
                innerException: innerException);
        }
    }
}