// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;

using cCoder.Core.Models.Exceptions;
using cCoder.Security.Models.Exceptions;

namespace cCoder.Core.Services.Aggregations.Security;

internal sealed partial class TenantAdministrationAggregationService
{
    private static async ValueTask TryCatch(Func<ValueTask> operation)
    {
        try
        {
            await operation();
        }
        catch (SecurityAggregationValidationException)
        {
            throw;
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
        catch (SecurityAggregationValidationException)
        {
            throw;
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
        catch (SecurityAggregationValidationException)
        {
            throw;
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