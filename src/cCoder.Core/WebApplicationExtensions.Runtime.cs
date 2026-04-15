using cCoder.ContentManagement.Exposures.Caching;
using cCoder.Core.Cors;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication UseCoreCaching(this WebApplication app)
    {
        app.Services.GetService<ICommonObjectCache>()?.Refresh();
        app.Services.GetService<IMetadataCache>()?.Rebuild();
        return app;
    }

    private static WebApplication UseCoreDefaultCors(this WebApplication app)
    {
        ICoreAllowedOriginStore originStore =
            app.Services.GetRequiredService<ICoreAllowedOriginStore>();

        app.UseCors(builder =>
        {
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.SetIsOriginAllowed(originStore.IsAllowed);
            builder.AllowCredentials();
        });

        return app;
    }

    private static WebApplication UseCoreExceptionHandling(this WebApplication app, RequestDelegate errorHandler)
    {
        app.UseExceptionHandler(errorApp => errorApp.Run(errorHandler));
        return app;
    }
}
