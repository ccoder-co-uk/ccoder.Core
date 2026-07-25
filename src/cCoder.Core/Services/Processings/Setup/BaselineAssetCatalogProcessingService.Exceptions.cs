// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Processings.Setup;

internal sealed partial class BaselineAssetCatalogProcessingService
{
    private static TResult TryCatch<TResult>(Func<TResult> operation)
    {
        try
        {
            return operation();
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