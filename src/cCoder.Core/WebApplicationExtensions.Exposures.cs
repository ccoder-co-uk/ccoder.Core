using System.Text.RegularExpressions;
using cCoder.Core.Exposures;
using cCoder.Core.Exposures.Hubs;
using cCoder.DocumentManagement.Exposures.Middleware;
using cCoder.Logging.Exposures.Hubs;
using cCoder.Workflow.Exposures.Hubs;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static readonly Regex DmsRouteRegex = new(@"^\/api\/dms.*", RegexOptions.Compiled);
    private static readonly Regex WebDavRouteRegex = new(@"^\/api\/webdav.*", RegexOptions.Compiled);

    private static WebApplication UseCoreApiShell(this WebApplication app)
    {
        StaticFileOptions defaultStaticFileOptions = new()
        {
            OnPrepareResponse = ctx =>
                ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + 86400,
        };

        app.UseStaticFiles(defaultStaticFileOptions);

        if (Directory.Exists("\\.well-known"))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider("\\.well-known"),
                RequestPath = new PathString("\\.well-known"),
                ServeUnknownFileTypes = true,
                OnPrepareResponse = defaultStaticFileOptions.OnPrepareResponse,
            });
        }

        app.UseRouting();
        app.MapStaticAssets();
        app.MapControllers();
        app.MapControllerRoute(
            name: "default",
            pattern: @"{*path}",
            defaults: new { controller = "Home", action = "Index" },
            constraints: new { path = new NoApiRouteConstraint() }
        );
        app.MapHub<NotificationHub>("/Api/Hubs/Notification");
        return app;
    }

    private static WebApplication UseContentManagementExposure(
        this WebApplication app,
        Func<HttpContext, ILogger, Task> onRequest,
        ILogger log = null
    )
    {
        log?.LogInformation("Initialising Content Management");
        app.UseSession();
        app.HandleExceptions();
        app.UseCoreFormatters();
        app.UseCoreCaching();
        app.Use(
            async (context, next) =>
            {
                await onRequest(context, log ?? NullLogger.Instance);

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
            }
        );
        return app;
    }

    private static WebApplication UseDocumentManagementExposure(
        this WebApplication app,
        ILogger log = null
    )
    {
        log?.LogInformation("Initialising Document Management");
        app.MapWhen(
            context => DmsRouteRegex.IsMatch(context.Request.Path.Value?.ToLower() ?? string.Empty),
            branch => branch.UseMiddleware<DMSMiddleware>()
        );

        app.MapWhen(
            context => WebDavRouteRegex.IsMatch(context.Request.Path.Value?.ToLower() ?? string.Empty),
            branch => branch.UseMiddleware<WebDavMiddleware>()
        );

        return app;
    }

    private static WebApplication UseWorkflowExposure(this WebApplication app, ILogger log = null)
    {
        log?.LogInformation("Initialising Workflow");
        app.MapHub<WorkflowHub>("/Api/Hubs/Workflow");
        return app;
    }

    private static WebApplication UseLoggingExposure(this WebApplication app, ILogger log = null)
    {
        log?.LogInformation("Initialising Logging");
        app.MapHub<LogHub>("/Api/Hubs/Logs");
        return app;
    }
}
