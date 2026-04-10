using System.Security;
using System.Web;
using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.Core;
using cCoder.Core.Api;
using cCoder.Core.Api.Hubs;
using cCoder.Data;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Data.Models;
using cCoder.DocumentManagement;
using cCoder.Mail;
using cCoder.Packaging;
using cCoder.Scheduling;
using cCoder.Security.Api;
using cCoder.Security.Api.EDM;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Objects;
using cCoder.Security.Objects.Entities;
using cCoder.Workflow;
using EventLibrary;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Web.Logging;
using Web.Services;
using Web.Services.Interfaces;
using ContentManagementConfig = cCoder.ContentManagement.Models.Config;
using MailConfig = cCoder.Mail.Models.Config;


namespace Web;

public class Program
{
    private static WebApplication app;
    private static ILogger log;
    private static string ssoConnection = "";

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ssoConnection = builder.Configuration.GetConnectionString("SSO");

        try
        {
            string coreConnection = builder.Configuration.GetConnectionString("Core");
            ODataConventionModelBuilder aggregateApiBuilder = new();
            var metadata = new Dictionary<string, IEdmModel>()
            {
                { "Security", new SecurityModelBuilder().Build().EDMModel }
            };

            Config config = new();
            builder.Configuration.Bind(config);
            builder.Services.AddSingleton(config);
            builder.Services.AddSingleton(
                new ContentManagementConfig
                {
                    ConnectionStrings = new Dictionary<string, string>(config.ConnectionStrings ?? new Dictionary<string, string>()),
                    Settings = new Dictionary<string, string>(config.Settings ?? new Dictionary<string, string>()),
                    Services = new Dictionary<string, string>(config.Services ?? new Dictionary<string, string>()),
                    DebugInfo = config.DebugInfo,
                    LogSQL = config.LogSQL,
                });
            builder.Services.AddSingleton(
                new MailConfig
                {
                    ConnectionStrings = new Dictionary<string, string>(config.ConnectionStrings ?? new Dictionary<string, string>()),
                    Settings = new Dictionary<string, string>(config.Settings ?? new Dictionary<string, string>()),
                    Services = new Dictionary<string, string>(config.Services ?? new Dictionary<string, string>()),
                    DebugInfo = config.DebugInfo,
                    LogSQL = config.LogSQL,
                });

            builder.Services.AddEventing();
            builder.Services.AddTransient<IUserRegistrationOrchestrationService, UserRegistrationOrchestrationService>();
            builder.Services.AddTransient<IUserPasswordOrchestrationService, UserPasswordOrchestrationService>();

            builder.Services.AddSecurityApi((services, securityConfig) =>
            {
                securityConfig.AddMSSQLModelProvider(
                    services,
                    builder.Configuration.GetConnectionString("SSO"));

                securityConfig.UseAESHMMACPasswordEncryption(
                    services,
                    builder.Configuration.GetSection("settings")["DecryptionKey"]);
            });
            builder.Services.AddSingleton(new ApiInfo
            {
                Kind = "Context",
                Name = "Security",
                Url = "Security",
                SwaggerDef = "/swagger/Security/swagger.json",
            });

            cCoder.Data.IServiceCollectionExtensions.AddCoreData(builder.Services, coreConnection);
            builder.Services.AddAppSecurityApi(aggregateApiBuilder);
            builder.Services.AddContentManagementApi(aggregateApiBuilder);
            builder.Services.AddDocumentManagementApi(aggregateApiBuilder);
            builder.Services.AddMailApi(aggregateApiBuilder);
            builder.Services.AddSchedulingApi(aggregateApiBuilder);
            builder.Services.AddWorkflowApi(aggregateApiBuilder);
            cCoder.Logging.IServiceCollectionExtensions.AddLoggingApi(builder.Services, aggregateApiBuilder);
            builder.Services.AddContentManagementInfrastructure(metadata);
            builder.Services.AddCoreApi(metadata);
            builder.Services.AddCoreApiDocumentation(
                "Security",
                "AppSecurity",
                "ContentManagement",
                "DocumentManagement",
                "Logging",
                "Mail",
                "Scheduling",
                "Workflow");

            builder.Services.AddTransient<SignalRLoggingBroker>();
            builder.Services.AddSingleton<ILoggerProvider, SignalRLoggingProvider>();

            builder.Logging.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
                options.SingleLine = true;
            });

            app = builder.Build();
            log = app.Services.GetService<ILogger<Program>>();

            app.UseHttpsRedirection();

        app.UseSwagger()
            .UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/Core/swagger.json", "Core API");
                options.SwaggerEndpoint("/swagger/Security/swagger.json", "Security API");
                options.SwaggerEndpoint("/swagger/AppSecurity/swagger.json", "AppSecurity API");
                options.SwaggerEndpoint("/swagger/ContentManagement/swagger.json", "ContentManagement API");
                options.SwaggerEndpoint("/swagger/DocumentManagement/swagger.json", "DocumentManagement API");
                options.SwaggerEndpoint("/swagger/Logging/swagger.json", "Logging API");
                options.SwaggerEndpoint("/swagger/Mail/swagger.json", "Mail API");
                options.SwaggerEndpoint("/swagger/Scheduling/swagger.json", "Scheduling API");
                options.SwaggerEndpoint("/swagger/Workflow/swagger.json", "Workflow API");
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Core API");
            })
            .UseODataBatching()
            .UseODataRouteDebug();

            SetupAspNet(app);

            app.UseAppSecurityExposure(log);
            cCoder.ContentManagement.WebApplicationExtensions.UseContentManagementExposure(
                app,
                LogRequest,
                log
            );
            app.UseMailExposure(log);
            cCoder.DocumentManagement.WebApplicationExtensions.UseDocumentManagementExposure(
                app,
                log
            );
            app.UsePackagingExposure(log);
            app.UseSchedulingExposure(log);
            cCoder.Workflow.WebApplicationExtensions.UseWorkflowExposure(app, log);
            cCoder.Logging.WebApplicationExtensions.UseLoggingExposure(app, log);
            app.UseCoreDefaultCors();
            app.UseCoreExceptionHandling(HandleUnHandledException);
            app.UseAppSecurityEventHandlers();
            app.ListenToContentManagementEvents();
            app.UseDocumentManagementEventHandlers();
            app.UseMailEventHandlers();
            app.UseSchedulingEventHandlers();
            app.UseWorkflowEventHandlers();
            app.UseAppSecurityDeleteEventHandlers();
            app.Run();
            log.LogInformation("System is running.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            if (Environment.UserInteractive && !Console.IsInputRedirected)
                Console.ReadKey();
        }
    }

    public static WebApplication SetupAspNet(WebApplication app)
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

    private static async Task HandleUnHandledException(HttpContext context)
    {

        Exception ex = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        context.Response.StatusCode = ex?.GetType() == typeof(SecurityException)
            ? 401
            : 500;

        context.Response.ContentType = "application/json";

        if (ex != null)
        {
            log.LogError($"{ex.Message}\n{ex.StackTrace}");

            await context.Response.WriteAsync("{ \"error\": \"" + ex.Message.Replace("\"", "\'") + "\" }");

            Exception innerEx = ex.InnerException;

            while (innerEx != null)
            {
                log.LogError($"{innerEx.Message}\n{innerEx.StackTrace}");
                innerEx = innerEx.InnerException;
            }
        }
    }

    private static async Task LogRequest(HttpContext context, ILogger log)
    {
        HttpRequest request = context.RequestServices.GetService<HttpRequest>();

        if (
            request != null
            && !request.Path.StartsWithSegments("/Api/Hubs", StringComparison.OrdinalIgnoreCase)
        )

        {
            ICoreAuthInfo authInfo = context.RequestServices.GetRequiredService<ICoreAuthInfo>();
            IContentManagementAppService appService = context.RequestServices.GetRequiredService<IContentManagementAppService>();

            string url = HttpUtility.UrlDecode(request.GetDisplayUrl());
            string logEntry = $"{context.Connection.RemoteIpAddress} as {authInfo.SSOUserId}: {request.Method} - {url}";

            if (context.Session != null)
            {
                try
                {
                    // this can't come from the service provider due to 1 per request configuration
                    // operations like logins cause threading problems.
                    using SecurityDbContext sso = new MSSQLSecurityDbContextFactory(ssoConnection)
                        .CreateDbContext();

                    string requestType = request.Path.Value.StartsWith("/api/", StringComparison.InvariantCultureIgnoreCase)
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
                    log.LogWarning("Unable to persist request log entry to SSO. {Message}", ex.Message);
                }
            }

            log.LogDebug(logEntry);
        }
    }
}







