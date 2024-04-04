using cCoder.Core.Api;
using cCoder.Core.Objects;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData;
using Microsoft.OpenApi.Models;
using System.Security;
using System.Web;

namespace Web
{
    public class Program
    {
        public static string SSOUserId = "Guest";

        public static void Main(string[] args)
        {
            var configRoot = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "ENV_")
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSimpleConsole(options => options.SingleLine = true);
                loggingBuilder.AddFilter(level => level >= LogLevel.Debug);
            });

            var log = builder.Services.BuildServiceProvider()
                .GetRequiredService<ILogger<Program>>();

            try
            {
                log.LogInformation("Logging initialised, beginning application construction ...");
                var config = new Config();
                configRoot.Bind(config);
                builder.Services.AddSingleton(config);

                log.LogInformation("Initialising Services ...");

                builder.Services.AddCore(coreConfig =>
                {
                    coreConfig
                        .UseMSSQLProvider(configRoot.GetConnectionString("cCoder.Core"))
                        .UseContentManagement()
                        .UseDocumentManagement()
                        .AuthorizeUsersWith(ctx => SSOUserId);
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

                log.LogInformation("Service Initialisation complete!");
                log.LogInformation("Building Application ...");

                var app = builder.Build();

                app
                    .UseCore(coreBulder =>
                    {
                        coreBulder
                            .UseContentManagement(LogRequest)
                            .UseDocumentManagement()
                            .HandleCorsWithDefaults()
                            .HandleExceptionsWith(HandleUnHandledException);
                    }, log)
                    .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Corporate LinX V7 API definition"))
                    .UseSwagger()
                    .UseODataRouteDebug();

                app.Run();
                log.LogInformation("System is running.");
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        static async Task HandleUnHandledException(HttpContext context)
        {
            Exception ex = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
            context.Response.StatusCode = ex?.GetType() == typeof(SecurityException) ? 401 : 500;
            context.Response.ContentType = "application/json";

            if (ex != null)
            {
                await context.Response.WriteAsync("{ \"error\": \"" + ex.Message.Replace("\"", "\'") + "\" }");

                Exception innerEx = ex.InnerException;

                while (innerEx != null)
                    innerEx = innerEx.InnerException;
            }
        }

        static Task LogRequest(HttpContext context, ILogger log)
        {
            var request = context.RequestServices.GetService<HttpRequest>();

            if (request != null && !request.GetDisplayUrl().Contains("/Api/Hub/"))
            {
                var core = context.RequestServices.GetService<ICoreDataContext>();
                var url = HttpUtility.UrlDecode(request.GetDisplayUrl());
                log.LogDebug($"{context.Connection.RemoteIpAddress} as {core.AuthInfo.SSOUserId}: {request.Method} - {url}");
            }

            return Task.CompletedTask;
        }
    }
}