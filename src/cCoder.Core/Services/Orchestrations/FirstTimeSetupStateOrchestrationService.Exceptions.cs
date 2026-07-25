// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class FirstTimeSetupStateOrchestrationService
{
    private static async Task<bool> TryCatch(
        Func<Task<bool>> operation)
    {
        try
        {
            return await operation();
        }
        catch (CoreValidationException innerException)
        {
            throw new CoreOrchestrationValidationException(
                innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreOrchestrationDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreOrchestrationServiceException(
                innerException: innerException);
        }
    }
}