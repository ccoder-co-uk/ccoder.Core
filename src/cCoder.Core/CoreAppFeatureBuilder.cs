using System.Text.RegularExpressions;
using cCoder.Core.Api;
using cCoder.DocumentManagement.Exposures.Middleware;
using cCoder.Logging.Exposures.Hubs;
using cCoder.Workflow.Exposures.Hubs;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;


namespace cCoder.Core;

public partial class CoreAppFeatureBuilder
{
    public CoreAppFeatureBuilder(WebApplication app, ILogger logger = null)
    {
        App = app;
        Logger = logger;
    }

    public WebApplication App { get; }

    public ILogger Logger { get; }

    public CoreAppFeatureBuilder UseContentManagement(Func<HttpContext, ILogger, Task> onRequest)
    {
        Logger?.LogInformation("Initialising Content Management");

        App.UseSession();
        App.HandleExceptions();
        App.UseCoreFormatters();
        App.UseCaching();
        App.UseRoutes();
        App.UseHubs();

        App.Use(
            async (context, next) =>
            {
                await onRequest(context, Logger ?? NullLogger.Instance);
                context.Response.OnStarting(() => RemovePlatformHeaders(context));
                await next();
            }
        );

        return this;
    }

    public CoreAppFeatureBuilder UseDocumentManagement() =>
        UseDocumentManagementCore();

    public CoreAppFeatureBuilder UseWorkflow() =>
        UseWorkflowCore();

    public CoreAppFeatureBuilder UseLogging() =>
        UseLoggingCore();

    public CoreAppFeatureBuilder HandleCorsWith(Action<CorsPolicyBuilder> corsPolicybuilder)
    {
        App.UseCors(corsPolicybuilder);
        return this;
    }

    public CoreAppFeatureBuilder HandleCorsWithDefaults()
    {
        App.UseCors(builder =>
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
        App.UseExceptionHandler(errorApp => errorApp.Run(errorHandler));
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

    private CoreAppFeatureBuilder UseDocumentManagementCore()
    {
        Logger?.LogInformation("Initialising Document Management");

        App.MapWhen(
            context => DmsRegex().IsMatch(context.Request.Path.Value?.ToLower() ?? string.Empty),
            branch => branch.UseMiddleware<DMSMiddleware>()
        );

        App.MapWhen(
            context => WebDavRegex().IsMatch(context.Request.Path.Value?.ToLower() ?? string.Empty),
            branch => branch.UseMiddleware<WebDavMiddleware>()
        );

        return this;
    }

    private CoreAppFeatureBuilder UseWorkflowCore()
    {
        Logger?.LogInformation("Initialising Workflow");
        App.MapHub<WorkflowHub>("/Api/Hubs/Workflow");
        return this;
    }

    private CoreAppFeatureBuilder UseLoggingCore()
    {
        Logger?.LogInformation("Initialising Logging");
        App.MapHub<LogHub>("/Api/Hubs/Logs");
        return this;
    }

    [GeneratedRegex(@"^\/api\/dms.*")]
    private static partial Regex DmsRegex();

    [GeneratedRegex(@"^\/api\/webdav.*")]
    private static partial Regex WebDavRegex();
}




