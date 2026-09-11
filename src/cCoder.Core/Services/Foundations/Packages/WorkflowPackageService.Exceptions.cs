// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Foundations.Packages;

internal sealed partial class WorkflowPackageService
{
    private static async ValueTask TryCatch(Func<ValueTask> operation)
    {
        try { await operation(); }
        catch (ArgumentException innerException) { throw new CoreValidationException(innerException); }
        catch (CoreDependencyException innerException) { throw new CoreDependencyException(innerException); }
        catch (Exception innerException) { throw new CoreServiceException(innerException); }
    }

    private static TResult TryCatch<TResult>(Func<TResult> operation)
    {
        try { return operation(); }
        catch (ArgumentException innerException) { throw new CoreValidationException(innerException); }
        catch (CoreDependencyException innerException) { throw new CoreDependencyException(innerException); }
        catch (Exception innerException) { throw new CoreServiceException(innerException); }
    }
}