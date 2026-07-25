// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Foundations.AllowedOrigins;

internal sealed partial class AllowedOriginStoreService
{
    private static async ValueTask<string[]> TryCatch(
        Func<ValueTask<string[]>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ArgumentException innerException)
        {
            throw new CoreValidationException(
                innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreServiceException(
                innerException: innerException);
        }
    }
}