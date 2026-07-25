// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;
using cCoder.Data.Models.Mail;

namespace cCoder.Core.Services.Foundations.Mail;

internal sealed partial class MailManagerService
{
    private static async ValueTask<QueuedEmail> TryCatch(
        Func<ValueTask<QueuedEmail>> operation)
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