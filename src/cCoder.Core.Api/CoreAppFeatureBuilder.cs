using cCoder.Core.Api.Middleware;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Text.RegularExpressions;

namespace cCoder.Core.Api;

public partial class CoreAppFeatureBuilder(WebApplication app, ILogger log = null)
{
    [GeneratedRegex(@"^\/api\/dms.*")]
    private static partial Regex DMSRegex();

    [GeneratedRegex(@"^\/api\/webdav.*")]
    private static partial Regex WebDAVRegex();

    public CoreAppFeatureBuilder UseContentManagement(Func<HttpContext, ILogger, Task> onRequest)
    {
        log?.LogInformation("Initialising Content Management");

        app.UseSession();
        app.HandleExceptions();
        app.UseCoreFormatters();
        app.UseCaching();
        app.UseRoutes();
        app.Usehubs();

        app.Use(async (context, next) =>
        {
            await onRequest(context, log);
            context.Response.OnStarting(() => RemovePlatformHeaders(context));
            await next();
        });

        return this;
    }

    public CoreAppFeatureBuilder UseDocumentManagement()
    {
        log?.LogInformation("Initialising Document Management");

        app.MapWhen(
            context => DMSRegex().IsMatch(context.Request.Path.Value.ToLower()),
            appBranch => appBranch.UseMiddleware<DMSMiddleware>()
        );

        app.MapWhen(
            context => WebDAVRegex().IsMatch(context.Request.Path.Value.ToLower()),
            appBranch => appBranch.UseMiddleware<WebDavMiddleware>()
        );

        return this;
    }


    public CoreAppFeatureBuilder HandleCorsWith(Action<CorsPolicyBuilder> corsPolicybuilder)
    {
        app.UseCors(corsPolicybuilder);
        return this;
    }

    public CoreAppFeatureBuilder HandleCorsWithDefaults()
    {
        app.UseCors(builder =>
        {
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.SetIsOriginAllowed(_ => true);
            builder.AllowCredentials();
        });

        return this;
    }

    public CoreAppFeatureBuilder HandleExceptionsWith(RequestDelegate errorHandler)
    {
        app.UseExceptionHandler(errorApp => errorApp.Run(errorHandler));
        return this;
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
}