// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Processings.Formatters;

internal sealed partial class ExcelFileProcessingService
{
    private static Stream TryCatch(Func<Stream> operation)
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