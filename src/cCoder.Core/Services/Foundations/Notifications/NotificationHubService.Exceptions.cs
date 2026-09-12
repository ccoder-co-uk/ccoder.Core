// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Foundations.Notifications;

internal sealed partial class NotificationHubService
{
    private static async Task TryCatch(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (ArgumentException innerException)
        {
            throw new CoreValidationException(innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreDependencyException(innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreServiceException(innerException: innerException);
        }
    }
}