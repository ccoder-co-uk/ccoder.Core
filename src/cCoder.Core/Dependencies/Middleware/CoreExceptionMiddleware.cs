// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;

namespace cCoder.Core.Dependencies.Middleware;

internal sealed class CoreExceptionMiddleware(
    ILogger<CoreExceptionMiddleware> log) : IMiddleware
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        try
        {
            await next(context: context);
        }
        catch (Exception exception)
        {
            context.Response.StatusCode =
                exception is SecurityException ? 401 : 500;

            context.Response.ContentType = "application/json";
            log.LogError(
                exception: exception,
                message: exception.Message);

            await context.Response.WriteAsync(
                text: "{ \"error\": \""
                    + exception.Message.Replace(
                        oldValue: "\"",
                        newValue: "\'")
                    + "\" }");
        }
    }
}