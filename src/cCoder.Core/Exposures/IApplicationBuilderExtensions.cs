// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Middleware;

namespace cCoder.Core;

internal static class IApplicationBuilderExtensions
{
    internal static IApplicationBuilder UseCoreFormatters(
        this IApplicationBuilder app) =>
        app.Use(
            middleware: async (context, next) =>
            {
                ICoreFormatterMiddlewareProcessingService service =
                    context.RequestServices.GetRequiredService<
                        ICoreFormatterMiddlewareProcessingService>();

                await service.ProcessAsync(
                    context: context,
                    next: next);
            });
}