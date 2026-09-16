// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.Core.Brokers.Loggings;
using Web.Models.Exceptions;

namespace Web.Dependencies.Middleware;

internal sealed class WebExceptionMiddleware(
    ILoggingBroker loggingBroker) : IMiddleware
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ApiCacheValidationException exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "Cache refresh request validation failed.");

            await WriteErrorAsync(
                context: context,
                statusCode: StatusCodes.Status400BadRequest,
                message: "The cache refresh request is invalid.");
        }
        catch (ApiScriptOrchestrationValidationException exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "Script request validation failed.");

            await WriteErrorAsync(
                context: context,
                statusCode: StatusCodes.Status400BadRequest,
                message: "The script request is invalid.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            text: JsonSerializer.Serialize(
                value: new
                {
                    error = message
                }));
    }
}