using cCoder.Core.Objects;
using HostedServices.Logging;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData;

namespace HostedServices;

public class Program
{
    private static WebApplication app;
    private static ILogger log;

    public static void Main(string[] args)
    {
        app = GetWebApplication(args);

        log = app.Services.GetService<ILogger<Program>>();

        app.UseSwagger()
            .UseSwaggerUI(c => c.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "Corporate LinX V7 API definition"))
            .UseODataBatching()
            .UseODataRouteDebug();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.UseRouting();

        app.UseCors(delegate (CorsPolicyBuilder builder)
        {
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.SetIsOriginAllowed((string _) => true);
            builder.AllowCredentials();
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            HttpsCompression = HttpsCompressionMode.Compress
        });

        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() => RemovePlatformHeaders(context));
            await next();
        });

        app.MapHub<LogHub>("/Hubs/Logs");

        app.Run();
        log.LogInformation("Ready to begin receiving Events!");
    }

    internal static WebApplication GetWebApplication(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .AddEnvironmentVariables()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.testing.json", optional: true, reloadOnChange: true);

        Config config = new();
        builder.Configuration.Bind(config);
        builder.Services.AddSingleton(config);

        ConfigureLogging(builder.Logging, builder.Configuration);
        builder.Services.ConfigureAllServices(builder.Configuration);

        return builder.Build();
    }

    private static void ConfigureLogging(ILoggingBuilder logBuilder, IConfiguration config)
    {
        logBuilder.ClearProviders();
        logBuilder.AddFilter(level => level >= LogLevel.Debug);

        logBuilder.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
            options.SingleLine = true;
        });

        logBuilder.AddConfiguration(config.GetSection("logging"));
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