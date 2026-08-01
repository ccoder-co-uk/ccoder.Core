// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Middleware;
using cCoder.Core.Models.Exceptions;

namespace cCoder.Core.Dependencies.Middleware;

internal sealed class CoreFormatterMiddleware(
    ICoreFormatterMiddlewareProcessingService processingService)
    : IMiddleware
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        try
        {
            await processingService.ProcessAsync(
                context: context,
                next: next);
        }
        catch (CoreProcessingValidationException)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;
        }
        catch (Exception)
        {
            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;
        }
    }
}