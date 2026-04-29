using cCoder.Core.Cors;
using cCoder.Logging.Exposures.Hubs;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    public static WebApplication StartCoreWeb(this WebApplication app)
    {
        ILogger log = app.Services
            .GetService<ILoggerFactory>()?
            .CreateLogger("cCoder.Core.Web")
            ?? NullLogger.Instance;

        app.Services.GetRequiredService<ICoreAllowedOriginStore>()
            .RefreshAsync()
            .GetAwaiter()
            .GetResult();

        app.UseHttpsRedirection();
        app.UseCoreApi(log);

        return app;
    }

    public static WebApplication StartCoreHostedServices(this WebApplication app)
    {
        app.Services.GetRequiredService<ICoreAllowedOriginStore>()
            .RefreshAsync()
            .GetAwaiter()
            .GetResult();

        app.ListenToExternalEvents();
        app.UseRouting();
        app.UseAuthorization();
        app.UseCoreDefaultCors();
        app.UseStaticFiles(new StaticFileOptions
        {
            HttpsCompression = HttpsCompressionMode.Compress,
        });
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                if (context.Request.Query["edit"] != "true")
                    context.Response.Headers.Append("X-Frame-Options", "DENY");

                _ = context.Response.Headers.Remove("X-AspNet-Version");
                _ = context.Response.Headers.Remove("X-AspNetMvc-Version");
                _ = context.Response.Headers.Remove("X-Sourcefiles");
                _ = context.Response.Headers.Remove("Server");

                return Task.CompletedTask;
            });
            await next();
        });
        app.MapControllers();
        app.MapHub<LogHub>("/Hubs/Logs");
        return app;
    }
}
