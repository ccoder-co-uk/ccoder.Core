// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models.Exceptions;

namespace Web.Services.Foundations.Api;

internal sealed partial class ApiScriptAuthorizationService
{
    private static void TryCatch(Action operation)
    {
        try
        {
            operation();
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