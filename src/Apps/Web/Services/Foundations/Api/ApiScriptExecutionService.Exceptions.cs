// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models.Exceptions;

namespace Web.Services.Foundations.Api;

internal sealed partial class ApiScriptExecutionService
{
    private static async ValueTask<string> TryCatch(
        Func<ValueTask<string>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ArgumentException innerException)
        {
            throw new ApiScriptValidationException(innerException);
        }
        catch (ApiScriptDependencyException innerException)
        {
            throw new ApiScriptDependencyException(innerException);
        }
        catch (Exception innerException)
        {
            throw new ApiScriptDependencyException(innerException);
        }
    }
}