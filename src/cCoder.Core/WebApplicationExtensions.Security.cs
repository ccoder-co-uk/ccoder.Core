// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication UseCoreSecurityHeaders(
        this WebApplication app)
    {
        Models.CoreConfiguration configuration =
            app.Services.GetService<Models.CoreConfiguration>();

        bool exposeApiMetadata =
            ShouldExposeApiSurface(
                configuredValue:
                    configuration?.Api?.ExposeMetadata,
                isProduction: app.Environment.IsProduction());

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.Use(middleware: async (context, next) =>
        {
            if (!exposeApiMetadata
                && context.Request.Path.Value?.EndsWith(
                    value: "/$metadata",
                    comparisonType:
                        StringComparison.OrdinalIgnoreCase) == true)
            {
                context.Response.StatusCode =
                    StatusCodes.Status404NotFound;

                return;
            }

            context.Response.OnStarting(callback: () =>
            {
                context.Response.Headers["X-Content-Type-Options"] =
                    "nosniff";
                context.Response.Headers["Referrer-Policy"] =
                    "no-referrer";

                _ = context.Response.Headers.Remove(key: "Server");
                _ = context.Response.Headers.Remove(
                    key: "X-Powered-By");

                return Task.CompletedTask;
            });

            await next();
        });

        return app;
    }

    internal static bool ShouldExposeApiSurface(
        bool? configuredValue,
        bool isProduction) =>
        configuredValue ?? !isProduction;
}