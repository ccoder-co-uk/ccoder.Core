using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Text.RegularExpressions;
using Web.Api.Middleware;

namespace Core.Api
{
    public class CoreAppFeatureBuilder
    {
        private readonly WebApplication app;
        private readonly ILogger log;

        public CoreAppFeatureBuilder(WebApplication app, ILogger log = null)
        {
            this.app = app;
            this.log = log;
        }

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
                context => Regex.IsMatch(context.Request.Path.Value.ToLower(), @"^\/api\/dms.*"),
                appBranch => appBranch.UseMiddleware<DMSMiddleware>()
            );

            app.MapWhen(
                context => Regex.IsMatch(context.Request.Path.Value.ToLower(), @"^\/api\/webdav.*"),
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

        Task RemovePlatformHeaders(HttpContext context)
        {
            if (context.Request.Query["edit"] != "true")
                context.Response.Headers.Add("X-Frame-Options", "DENY");

            _ = context.Response.Headers.Remove("X-AspNet-Version");
            _ = context.Response.Headers.Remove("X-AspNetMvc-Version");
            _ = context.Response.Headers.Remove("X-Sourcefiles");

            return Task.CompletedTask;
        }
    }
}