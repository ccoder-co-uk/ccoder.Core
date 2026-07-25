// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal sealed partial class TemplatedEmailContentService
{
    private static TResult TryCatch<TResult>(Func<TResult> operation)
    {
        try
        {
            return operation();
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