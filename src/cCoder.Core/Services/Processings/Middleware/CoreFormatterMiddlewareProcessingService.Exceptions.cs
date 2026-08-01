// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Processings.Middleware;

internal sealed partial class CoreFormatterMiddlewareProcessingService
{
    private static async Task TryCatch(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (CoreValidationException innerException)
        {
            throw new CoreProcessingValidationException(
                innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreProcessingDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreProcessingServiceException(
                innerException: innerException);
        }
    }
}