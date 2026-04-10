using System.Text.RegularExpressions;
using cCoder.ContentManagement.Exposures.EventHandlers;
using cCoder.Core.Api;
using cCoder.Core.Api.Hubs;
using cCoder.AppSecurity.Exposures.EventHandlers;
using cCoder.DocumentManagement.Exposures.EventHandlers;
using cCoder.DocumentManagement.Exposures.Middleware;
using cCoder.Logging.Exposures.Hubs;
using cCoder.Mail.Exposures.EventHandlers;
using cCoder.Scheduling.Exposures.EventHandlers;
using cCoder.Workflow.Exposures.EventHandlers;
using cCoder.Workflow.Exposures.Hubs;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;


namespace cCoder.Core;

public static class WebApplicationExtensions
{
    private static readonly Regex DmsRouteRegex = new(@"^\/api\/dms.*", RegexOptions.Compiled);
    private static readonly Regex WebDavRouteRegex = new(@"^\/api\/webdav.*", RegexOptions.Compiled);

    public static WebApplication UseCoreApiShell(this WebApplication app)
    {
        StaticFileOptions options = new()
        {
            OnPrepareResponse = ctx =>
                ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + 86400,
        };

        if (Directory.Exists("\\.well-known"))
        {
            options.FileProvider = new PhysicalFileProvider("\\.well-known");
            options.RequestPath = new PathString("\\.well-known");
            options.ServeUnknownFileTypes = true;
        }

        app.UseStaticFiles(options);
        app.UseRouting();
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

    public static WebApplication UseCoreCaching(this WebApplication app)
    {
        app.Services.GetService<cCoder.ContentManagement.Exposures.Caching.ICommonObjectCache>()?.Refresh();
        app.Services.GetService<cCoder.ContentManagement.Exposures.Caching.IMetadataCache>()?.Rebuild();
        return app;
    }

    public static WebApplication UseContentManagementExposure(
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
                context.Response.OnStarting(() => RemovePlatformHeaders(context));
                await next();
            }
        );
        return app;
    }

    private static Task RemovePlatformHeaders(HttpContext context)
    {
        if (context.Request.Query["edit"] != "true")
            context.Response.Headers.Append("X-Frame-Options", "DENY");

        _ = context.Response.Headers.Remove("X-AspNet-Version");
        _ = context.Response.Headers.Remove("X-AspNetMvc-Version");
        _ = context.Response.Headers.Remove("X-Sourcefiles");
        _ = context.Response.Headers.Remove("Server");

        return Task.CompletedTask;
    }

    public static WebApplication UseDocumentManagementExposure(
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

    public static WebApplication UseWorkflowExposure(this WebApplication app, ILogger log = null)
    {
        log?.LogInformation("Initialising Workflow");
        app.MapHub<WorkflowHub>("/Api/Hubs/Workflow");
        return app;
    }

    public static WebApplication UseLoggingExposure(this WebApplication app, ILogger log = null)
    {
        log?.LogInformation("Initialising Logging");
        app.MapHub<LogHub>("/Api/Hubs/Logs");
        return app;
    }

    public static WebApplication UseCoreDefaultCors(this WebApplication app)
    {
        app.UseCors(builder =>
        {
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.SetIsOriginAllowed(_ => true);
            builder.AllowCredentials();
        });

        return app;
    }

    public static WebApplication UseCoreExceptionHandling(
        this WebApplication app,
        RequestDelegate errorHandler
    )
    {
        app.UseExceptionHandler(errorApp => errorApp.Run(errorHandler));
        return app;
    }

    public static WebApplication UseCoreEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        foreach (IContentManagementEventHandlers handlers in services.GetServices<IContentManagementEventHandlers>())
            handlers.ListenToAllEvents();

        foreach (IDocumentManagementEventHandlers handlers in services.GetServices<IDocumentManagementEventHandlers>())
            handlers.ListenToAllEvents();

        foreach (IMailEventHandlers handlers in services.GetServices<IMailEventHandlers>())
            handlers.ListenToAllEvents();

        foreach (ISchedulingEventHandlers handlers in services.GetServices<ISchedulingEventHandlers>())
            handlers.ListenToAllEvents();

        foreach (IWorkflowEventHandlers handlers in services.GetServices<IWorkflowEventHandlers>())
            handlers.ListenToAllEvents();

        foreach (IAppSecurityEventHandlers handlers in services.GetServices<IAppSecurityEventHandlers>())
            handlers.ListenToAllEvents();

        return app;
    }

    public static WebApplication UseCore(
        this WebApplication app,
        Action<CoreAppFeatureBuilder> coreFeatureBuilderAction,
        ILogger log = null
    )
    {
        log?.LogInformation("Setting up cCoder.Core ...");

        CoreAppFeatureBuilder coreFeatureBuilder = new(app, log);

        app.UseCoreApiShell();

        coreFeatureBuilderAction(coreFeatureBuilder);
        app.UseCoreEventHandlers();

        log?.LogInformation("Core is Ready!");

        return app;
    }

    internal static void UseCaching(this WebApplication app)
    {
        app.UseCoreCaching();
    }

    internal static void UseRoutes(this WebApplication app)
    {
        app.MapControllers();

        app.MapControllerRoute(
            name: "default",
            pattern: @"{*path}",
            defaults: new { controller = "Home", action = "Index" },
            constraints: new { path = new NoApiRouteConstraint() }
        );
    }

    internal static void UseHubs(this WebApplication app)
    {
        app.MapHub<NotificationHub>("/Api/Hubs/Notification");
        app.MapHub<WorkflowHub>("/Api/Hubs/Workflow");
        app.MapHub<LogHub>("/Api/Hubs/Logs");
    }

    internal static void UseEventHandlers(this WebApplication app)
    {
        app.UseCoreEventHandlers();
    }
}




