// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class AppAggregationService
{
    private static async ValueTask<App> TryCatch(
        Func<ValueTask<App>> operation)
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
}