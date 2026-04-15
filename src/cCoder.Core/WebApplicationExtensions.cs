using System.Security;
using System.Web;
using cCoder.AppSecurity;
using cCoder.Core.Cors;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Data;
using cCoder.Logging.Exposures.Hubs;
using cCoder.Mail;
using cCoder.Packaging;
using cCoder.Scheduling;
using cCoder.Security.Data.EF;
using cCoder.Security.Objects.Entities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
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

    private static WebApplication UseCoreApi(
        this WebApplication app,
        ILogger log = null)
    {
        app.UseCoreApiDocumentation();
        app.UseCoreApiShell();
        app.UseAppSecurityExposure(log);
        cCoder.ContentManagement.WebApplicationExtensions.UseContentManagementExposure(
            app,
            LogRequest,
            log);
        app.UseMailExposure(log);
        cCoder.DocumentManagement.WebApplicationExtensions.UseDocumentManagementExposure(app, log);
        app.UsePackagingExposure(log);
        app.UseSchedulingExposure(log);
        cCoder.Workflow.WebApplicationExtensions.UseWorkflowExposure(app, log);
        cCoder.Logging.WebApplicationExtensions.UseLoggingExposure(app, log);
        app.UseCoreDefaultCors();
        app.UseCoreExceptionHandling(HandleUnhandledException);
        app.UseCoreEventHandlers();
        return app;
    }

    static async Task HandleUnhandledException(HttpContext context)
    {
        ILogger logger = context.RequestServices
            .GetService<ILoggerFactory>()?
            .CreateLogger("cCoder.Core.Web")
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        Exception exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        context.Response.StatusCode =
            exception?.GetType() == typeof(SecurityException) ? 401 : 500;

        context.Response.ContentType = "application/json";

        if (exception is null)
            return;

        logger.LogError("{Message}\n{StackTrace}", exception.Message, exception.StackTrace);
        await context.Response.WriteAsync(
            "{ \"error\": \"" + exception.Message.Replace("\"", "\'") + "\" }");

        Exception innerException = exception.InnerException;

        while (innerException is not null)
        {
            logger.LogError("{Message}\n{StackTrace}", innerException.Message, innerException.StackTrace);
            innerException = innerException.InnerException;
        }
    }

    static async Task LogRequest(HttpContext context, ILogger logger)
    {
        HttpRequest request = context.RequestServices.GetService<HttpRequest>();

        if (request is null
            || request.Path.StartsWithSegments("/Api/Hubs", StringComparison.OrdinalIgnoreCase))
            return;

        ICoreAuthInfo authInfo = context.RequestServices.GetRequiredService<ICoreAuthInfo>();
        IContentManagementAppService appService =
            context.RequestServices.GetRequiredService<IContentManagementAppService>();
        Config config = context.RequestServices.GetRequiredService<Config>();

        string url = HttpUtility.UrlDecode(request.GetDisplayUrl());
        string logEntry =
            $"{context.Connection.RemoteIpAddress} as {authInfo.SSOUserId}: {request.Method} - {url}";

        if (context.Session is not null
            && config.ConnectionStrings?.TryGetValue("SSO", out string ssoConnectionString) == true
            && !string.IsNullOrWhiteSpace(ssoConnectionString))
        {
            try
            {
                using SecurityDbContext sso = new MSSQLSecurityDbContextFactory(ssoConnectionString)
                    .CreateDbContext();

                string requestType =
                    request.Path.Value?.StartsWith("/api/", StringComparison.InvariantCultureIgnoreCase) == true
                        ? "Api_"
                        : "Page_";

                UserEvent userEvent = new()
                {
                    TenantId = appService.GetByDomain(request.Host.Host, ignoreFilters: true)?.TenantId,
                    CreatedBy = authInfo.SSOUserId,
                    EventName = $"{requestType}{request.Method}{request.Path.Value}",
                    CreatedOn = DateTimeOffset.UtcNow,
                    SessionId = context.Session.Id,
                    Value = url,
                };

                await sso.AddAsync(userEvent);
                await sso.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    "Unable to persist request log entry to SSO. {Message}",
                    ex.Message);
            }
        }

        logger.LogDebug(logEntry);
    }
}
