// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Processings.Middleware;

internal interface ICoreFormatterMiddlewareProcessingService
{
    Task ProcessAsync(
        HttpContext context,
        RequestDelegate next);
}