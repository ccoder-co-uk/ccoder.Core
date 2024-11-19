using cCoder.Core.Api;
using cCoder.Core.Api.Formatters;
using cCoder.Security.Api;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Objects;
using cCoder.Security.Services;
using EventLibrary;
using EventLibrary.Objects.Interfaces;
using HostedServices.Brokers;
using HostedServices.Brokers.Interfaces;
using HostedServices.Logging;
using HostedServices.Services;
using HostedServices.Services.Interfaces;
using HostedServices.Services.Scheduled;
using HostedServices.Services.Scheduled.Interfaces;
using HostedServices.Services.Scheduled.Tasks;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

namespace HostedServices;

public static class IServiceCollectionExtensions
{
    public static void ConfigureAllServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient(ctx => ctx.CreateScope());
        services.AddTransient<SignalRLoggingBroker>();
        services.AddSingleton<ILoggerProvider, SignalRLoggingProvider>();

        ConfigureBusinessServices(services);

        services.AddEventing(ctx => ctx.GetService<ISSOAuthInfo>().SSOUserId);

        services.AddSwaggerGen(c =>
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

        ConfigureServices(services, config);
        ConfigureHostedServices(services, config);
        ConfigureApi(services);
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSecurityServices((services, securityConfig) =>
        {
            securityConfig.RootPath = "Api/Security";

            securityConfig.AddMSSQLModelProvider(
                services,
                config.GetConnectionString("SSO"));

            securityConfig.UseAESHMMACPasswordEncryption(
                services,
                config.GetSection("settings")["DecryptionKey"]);
        });

        services.AddCore(coreConfig =>
        {
            coreConfig.UseMSSQLProvider(config.GetConnectionString("Core"))
                .UseContentManagement(servicesOnly: true)
                .UseDocumentManagement()
                .AuthorizeUsersWith(ctx => ctx.GetService<IEventAuthInfo>().SSOUserId);
        });
    }

    private static void ConfigureBusinessServices(IServiceCollection services)
    {
        services.AddTransient<IEventBroker, EventBroker>();
        services.AddTransient<IMigrationService, MigrationService>();
    }

    private static void ConfigureHostedServices(IServiceCollection services, IConfiguration config)
    {
        //services.AddTransient<IScheduledOperation, DemoRunner>();
        services.AddTransient<IScheduledDailyOperation, AnalysePlatformUsage>();
        services.AddTransient<IScheduled1MinuteOperation, MailSender>();
        services.AddTransient<IScheduled1MinuteOperation, TaskRunner>();
        services.AddTransient<IScheduled1MinuteOperation, TokenCleaner>();
        services.AddTransient<IScheduled1MinuteOperation, WorkflowInstanceManagement>();
        services.AddTransient<IWorkflowInstanceManagement, WorkflowInstanceManagement>();

        services.AddHostedService<SchedulerHostedService>();
    }

    private static void ConfigureApi(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSession();
        services.AddTransient(ctx => ctx.GetService<IHttpContextAccessor>()?.HttpContext);
        services.AddTransient(ctx => ctx.GetService<HttpContext>()?.Request);

        services.AddTransient(ctx =>
        {
            try
            {
                return ctx.GetService<HttpContext>()?.Session;
            }
            catch
            {
                return null;
            }
        });

        services.AddHttpClient();

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

        services.AddControllers();
    }
}
