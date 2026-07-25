// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models.Exceptions;

namespace Web.Services.Aggregations;

internal sealed partial class ApiCacheAggregationService
{
    private static void TryCatch(
        Action operation)
    {
        try
        {
            operation();
        }
        catch (ApiCacheValidationException innerException)
        {
            throw new ApiCacheValidationException(
                innerException: innerException);
        }
        catch (ApiCacheDependencyException innerException)
        {
            throw new ApiCacheDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new ApiCacheServiceException(
                innerException: innerException);
        }
    }
}