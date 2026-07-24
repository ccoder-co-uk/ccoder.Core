// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures.Caching;
using cCoder.Core.Exposures.Cors;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private const string IsCoreCorsOriginAllowed = "cCoder.Core.Exposures.Cors.IsOriginAllowed";

    private static WebApplication UseCoreCaching(this WebApplication app)
    {
        app.Services.GetService<ICommonObjectCache>()?.Refresh();
        app.Services.GetService<IMetadataCache>()?.Rebuild();
        return app;
    }

    private static WebApplication UseCoreDefaultCors(this WebApplication app)
    {
        IHttpContextAccessor httpContextAccessor =
            app.Services.GetRequiredService<IHttpContextAccessor>();

        app.Use(middleware: async (context, next) =>
        {
            string origin = context.Request.Headers.Origin.ToString();

            if (!string.IsNullOrWhiteSpace(value: origin))
            {
                ICoreAllowedOriginStore originStore =
                    context.RequestServices.GetRequiredService<ICoreAllowedOriginStore>();

                context.Items[IsCoreCorsOriginAllowed] = await originStore.IsAllowedAsync(origin: origin);
            }

            await next();
        });

        app.UseCors(configurePolicy: builder =>
        {
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();

            builder.SetIsOriginAllowed(isOriginAllowed: _ =>
                httpContextAccessor.HttpContext?.Items.TryGetValue(
key: IsCoreCorsOriginAllowed, value: out object isAllowed) == true
                && isAllowed is true);

            builder.AllowCredentials();
        });

        return app;
    }

    private static WebApplication UseCoreExceptionHandling(this WebApplication app, RequestDelegate errorHandler)
    {
        app.UseExceptionHandler(configure: errorApp => errorApp.Run(handler: errorHandler));
        return app;
    }
}