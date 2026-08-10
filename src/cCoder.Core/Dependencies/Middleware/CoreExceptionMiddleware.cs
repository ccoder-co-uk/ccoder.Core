// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Dependencies.Middleware;

internal sealed class CoreExceptionMiddleware(
    ILoggingBroker log) : IMiddleware
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        try
        {
            await next(context: context);
        }
        catch (CoreProcessingValidationException exception)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            log.LogWarning(
                exception: exception,
                message: "Request validation failed.");
        }
        catch (SecurityException exception)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            log.LogWarning(
                exception: exception,
                message: "Request authentication failed.");
        }
        catch (Exception exception)
        {
            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            log.LogError(
                exception: exception,
                message: "Unhandled request exception.");
        }
    }
}