// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Middleware;

namespace cCoder.Core;

internal static class IApplicationBuilderExtensions
{
    internal static IApplicationBuilder UseCoreFormatters(
        this IApplicationBuilder app) =>
        app.UseMiddleware<CoreFormatterMiddleware>();

    internal static IApplicationBuilder HandleExceptions(
        this IApplicationBuilder app) =>
        app.UseMiddleware<CoreExceptionMiddleware>();
}