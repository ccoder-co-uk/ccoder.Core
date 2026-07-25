// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models.Exceptions;

namespace Web.Services.Orchestrations.Api;

internal sealed partial class ApiScriptOrchestrationService
{
    private static async ValueTask<string> TryCatch(
        Func<ValueTask<string>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ApiScriptValidationException innerException)
        {
            throw new ApiScriptOrchestrationValidationException(
                innerException: innerException);
        }
        catch (ApiScriptDependencyException innerException)
        {
            throw new ApiScriptOrchestrationDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new ApiScriptOrchestrationServiceException(
                innerException: innerException);
        }
    }
}