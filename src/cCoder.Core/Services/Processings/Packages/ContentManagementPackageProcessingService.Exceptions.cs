// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class ContentManagementPackageProcessingService
{
    private static async ValueTask TryCatch(Func<ValueTask> operation)
    {
        try { await operation(); }
        catch (CoreValidationException innerException) { throw new CoreProcessingValidationException(innerException); }
        catch (CoreDependencyException innerException) { throw new CoreProcessingDependencyException(innerException); }
        catch (Exception innerException) { throw new CoreProcessingServiceException(innerException); }
    }

    private static T TryCatch<T>(Func<T> operation)
    {
        try { return operation(); }
        catch (CoreValidationException innerException) { throw new CoreProcessingValidationException(innerException); }
        catch (CoreDependencyException innerException) { throw new CoreProcessingDependencyException(innerException); }
        catch (Exception innerException) { throw new CoreProcessingServiceException(innerException); }
    }
}