// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models.Exceptions;

namespace Web.Services.Foundations.Api;

internal sealed partial class ApiContextService
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
            throw new ApiScriptValidationException(
                innerException: innerException);
        }
        catch (ApiScriptDependencyException innerException)
        {
            throw new ApiScriptDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new ApiScriptDependencyException(
                innerException: innerException);
        }
    }

    private static async ValueTask<string> TryCatch(
        Func<ValueTask<string>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ArgumentException innerException)
        {
            throw new ApiScriptValidationException(
                innerException: innerException);
        }
        catch (ApiScriptDependencyException innerException)
        {
            throw new ApiScriptDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new ApiScriptDependencyException(
                innerException: innerException);
        }
    }
}