// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Processings.Setup;

internal sealed partial class SetupRequestHostProcessingService
{
    private static T TryCatch<T>(Func<T> operation)
    {
        try
        {
            return operation();
        }
        catch (ArgumentException innerException)
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