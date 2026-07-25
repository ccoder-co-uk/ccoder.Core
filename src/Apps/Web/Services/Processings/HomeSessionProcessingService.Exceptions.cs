// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models.Exceptions;

namespace Web.Services.Processings;

internal sealed partial class HomeSessionProcessingService
{
    private static T TryCatch<T>(
        Func<T> operation)
    {
        try
        {
            return operation();
        }
        catch (ArgumentException innerException)
        {
            throw new HomeSessionProcessingValidationException(
                innerException: innerException);
        }
        catch (HomeSessionProcessingDependencyException innerException)
        {
            throw new HomeSessionProcessingDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new HomeSessionProcessingServiceException(
                innerException: innerException);
        }
    }

    private static void TryCatch(
        Action operation)
    {
        try
        {
            operation();
        }
        catch (ArgumentException innerException)
        {
            throw new HomeSessionProcessingValidationException(
                innerException: innerException);
        }
        catch (HomeSessionProcessingDependencyException innerException)
        {
            throw new HomeSessionProcessingDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new HomeSessionProcessingServiceException(
                innerException: innerException);
        }
    }
}