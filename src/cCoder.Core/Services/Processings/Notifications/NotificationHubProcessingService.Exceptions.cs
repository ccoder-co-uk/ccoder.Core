// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Processings.Notifications;

internal sealed partial class NotificationHubProcessingService
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

    private static void TryCatch(Action operation)
    {
        try
        {
            operation();
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