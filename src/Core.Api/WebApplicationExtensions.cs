using cCoder.Core.Api.Hubs;
using cCoder.Core.Objects;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace cCoder.Core.Api
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseCore(this WebApplication app, Action<CoreAppFeatureBuilder> coreFeaturebuilderAction, ILogger log = null)
        {
            log?.LogInformation("Setting up cCoder.Core ...");

            var coreFeatureBuilder = new CoreAppFeatureBuilder(app, log);

            var options = new StaticFileOptions
            {
                OnPrepareResponse = ctx => ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + 86400
            };

            if (Directory.Exists("\\.well-known"))
            {
                options.FileProvider = new PhysicalFileProvider("\\.well-known");
                options.RequestPath = new PathString("\\.well-known");
                options.ServeUnknownFileTypes = true;
            }

            app.UseStaticFiles(options);
            app.UseRouting();
            app.UseHttpsRedirection();

            coreFeaturebuilderAction(coreFeatureBuilder);

            log?.LogInformation("cCoder.Core is Ready!");

            return app;
        }

        internal static void UseCaching(this WebApplication app)
        {
            ContentHelper.ObjectCache = app.Services.GetService<ICommonObjectCache>();
            ContentHelper.MetaCache = app.Services.GetService<IMetadataCache>();
            ContentHelper.ObjectCache?.Refresh();
        }

        internal static void UseRoutes(this WebApplication app)
        {
            app.MapControllerRoute(name: "api", pattern: "api/{controller=Root}/{action=Index}");

            app.MapControllerRoute(
                name: "default",
                pattern: @"{*path}",
                defaults: new { controller = "Home", action = "Index" },
                constraints: new { path = new NoApiRouteConstraint() });
        }

        internal static void Usehubs(this WebApplication app)
        {
            app.MapHub<NotificationHub>("/Api/Hubs/Notification");
            app.MapHub<WorkflowHub>("/Api/Hubs/Workflow");
            app.MapHub<LogHub>("/Api/Hubs/Logs");
        }
    }
}