using cCoder.Core.Api;
using cCoder.Core.Api.Formatters;
using cCoder.Core.Api.OData;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Security.Api;
using cCoder.Security.Api.EDM;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Objects;
using cCoder.Security.Objects.Entities;
using EventLibrary;
using EventLibrary.Objects.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OpenApi;
using System.Security;
using System.Web;
using Web.Logging;
using Web.Services;
using Web.Services.Interfaces;

namespace Web;

public class Program
{
    private static WebApplication app;
    private static IEventHub externalEventHub;
    private static ILogger log;
    private static string ssoConnection = "";

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ssoConnection = builder.Configuration.GetConnectionString("SSO");

        try
        {
            var metadata = new Dictionary<string, IEdmModel>()
            {
                { "Security", new SecurityModelBuilder().Build().EDMModel }
            };

            Config config = new();
            builder.Configuration.Bind(config);
            builder.Services.AddSingleton(config);

            builder.Services.AddEventing(serviceProvider => serviceProvider.GetService<ISSOAuthInfo>().SSOUserId);
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

            builder.Services.AddSwaggerGen(c =>
            {
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Description = @"Authorization header using the Bearer scheme. \r\n\r\n 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        \r\n\r\nExample: 'bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "bearer"
                });
            });

            builder.Services.AddCore(coreConfig =>
            {
                coreConfig
                    .UseMSSQLProvider(builder.Configuration.GetConnectionString("Core"))
                    .UseContentManagement(metadata, true)
                    .UseDocumentManagement()
                    .AuthorizeUsersWith(ctx => ctx.GetService<IEventAuthInfo>().SSOUserId);
            });

            AddApi(builder.Services);

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
                .UseSwaggerUI(c => c.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "Corporate LinX V7 API definition"))
                .UseODataBatching()
                .UseODataRouteDebug();

            app.UseCore(coreBulder => coreBulder
                .UseContentManagement(LogRequest)
                .UseDocumentManagement()
                .HandleCorsWithDefaults()
                .HandleExceptionsWith(HandleUnHandledException), log);

            app.Run();
            log.LogInformation("System is running.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            Console.ReadKey();
        }
    }

    private static void AddApi(IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromMinutes(60);
        });

        services.AddMvc(options =>
        {
            options.EnableEndpointRouting = false;
            options.OutputFormatters.Add(new XmlFormatter());
            options.OutputFormatters.Add(new CsvFormatter());
            options.OutputFormatters.Add(new ExcelFormatter());
        });

        services.Configure<KestrelServerOptions>(options =>
        {
            // if not set default value is: 30 MB
            options.Limits.MaxRequestBodySize = int.MaxValue;
        });

        services.AddRazorPages();
        services.AddEndpointsApiExplorer();
        services.AddSignalR();

        DefaultODataBatchHandler batchHandler = new();

        services.AddControllers()
            .AddOData(opt =>
            {
                opt.RouteOptions.EnableQualifiedOperationCall = false;
                opt.EnableAttributeRouting = true;

                opt
                    .Expand().Count().Filter().Select().OrderBy().SetMaxTop(1000)
                    .AddRouteComponents($"Api/Core", new CoreModelBuilder().Build().EDMModel, batchHandler)
                    .AddRouteComponents($"Api/Security", new SecurityModelBuilder().Build().EDMModel, batchHandler);
            });
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

        if (request != null && !request.GetDisplayUrl().Contains("/Api/Hub"))
        {
            // this can't come from the service provider due to 1 per request configuration
            // operations like logins cause threading problems.

            SecurityDbContext sso = new MSSQLSecurityDbContextFactory(ssoConnection)
                    .CreateDbContext();

            ICoreDataContext core = context.RequestServices.GetService<ICoreDataContext>();

            string url = HttpUtility.UrlDecode(request.GetDisplayUrl());
            string logEntry = $"{context.Connection.RemoteIpAddress} as {core.AuthInfo.SSOUserId}: {request.Method} - {url}";

            if (context.Session != null)
            {
                try
                {
                    string requestType = request.Path.Value.StartsWith("/api/", StringComparison.InvariantCultureIgnoreCase)
                        ? "Api_"
                        : "Page_";

                    UserEvent userEvent = new()
                    {
                        TenantId = core.GetAll<App>().FirstOrDefault(a => a.Domain == request.Host.Host)?.TenantId,
                        CreatedBy = core.AuthInfo.SSOUserId,
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
                    log.LogError($"{ex.Message}\n{ex.StackTrace}");
                }
            }

            log.LogDebug(logEntry);
        }
    }
}