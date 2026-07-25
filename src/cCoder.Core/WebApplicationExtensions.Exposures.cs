// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.RegularExpressions;
using cCoder.Core.Exposures;
using cCoder.Core.Dependencies.Hubs;
using cCoder.DocumentManagement.Exposures.Middleware;
using cCoder.Workflow;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    [GeneratedRegex(@"^\/api\/dms.*")]
    private static partial Regex GetDmsRouteRegex();

    [GeneratedRegex(@"^\/api\/webdav.*")]
    private static partial Regex GetWebDavRouteRegex();

    private static WebApplication UseCoreApiShell(this WebApplication app)
    {
        StaticFileOptions defaultStaticFileOptions = new()
        {
            OnPrepareResponse = ctx =>
                ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + 86400,
        };

        app.UseStaticFiles(options: defaultStaticFileOptions);

        if (Directory.Exists(path: "\\.well-known"))
        {
            app.UseStaticFiles(options: new StaticFileOptions
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

        app.MapHub<NotificationHub>(pattern: "/Api/Hubs/Notification");
        return app;
    }

    private static WebApplication UseContentManagementExposure(
        this WebApplication app,
        Func<HttpContext, ILogger, Task> onRequest,
        ILogger log = null
    )
    {
        log?.LogInformation(message: "Initialising Content Management");
        app.UseSession();
        app.HandleExceptions();
        app.UseCoreFormatters();
        app.UseCoreCaching();

        app.Use(
middleware: async (context, next) =>
            {
                await onRequest(arg1: context, arg2: log ?? NullLogger.Instance);

                context.Response.OnStarting(callback: () =>
                {
                    if (context.Request.Query["edit"] != "true")
                    {
                        context.Response.Headers.Append(key: "X-Frame-Options", value: "DENY");
                    }

                    _ = context.Response.Headers.Remove(key: "X-AspNet-Version");
                    _ = context.Response.Headers.Remove(key: "X-AspNetMvc-Version");
                    _ = context.Response.Headers.Remove(key: "X-Sourcefiles");
                    _ = context.Response.Headers.Remove(key: "Server");

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
        log?.LogInformation(message: "Initialising Document Management");

        app.MapWhen(
predicate: context => GetDmsRouteRegex().IsMatch(input: context.Request.Path.Value?.ToLower() ?? string.Empty), configuration: branch => branch.UseMiddleware<DMSMiddleware>()
        );

        app.MapWhen(
predicate: context => GetWebDavRouteRegex().IsMatch(input: context.Request.Path.Value?.ToLower() ?? string.Empty), configuration: branch => branch.UseMiddleware<WebDavMiddleware>()
        );

        return app;
    }

    private static WebApplication UseWorkflowExposure(this WebApplication app, ILogger log = null)
    {
        log?.LogInformation(message: "Initialising Workflow");
        return app.StartWorkflowWeb(log: log);
    }

}