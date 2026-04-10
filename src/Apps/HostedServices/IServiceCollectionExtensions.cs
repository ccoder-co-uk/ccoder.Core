using cCoder.AppSecurity;
using cCoder.Core;
using cCoder.Core.Api;
using cCoder.Data;
using cCoder.Mail;
using cCoder.Scheduling;
using cCoder.Security.Api;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Objects;
using cCoder.Security.Services;
using cCoder.Workflow;
using EventLibrary;
using HostedServices.Brokers;
using HostedServices.Brokers.Interfaces;
using HostedServices.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;


namespace HostedServices;

public static class IServiceCollectionExtensions
{
    public static void ConfigureAllServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<SignalRLoggingBroker>();
        services.AddSingleton<ILoggerProvider, SignalRLoggingProvider>();
        services.AddEventing();
        ConfigureServices(services, config);
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        string coreConnection = config.GetConnectionString("Core");

        services.AddSecurityServices((services, securityConfig) =>
        {
            securityConfig.AddMSSQLModelProvider(
                services,
                config.GetConnectionString("SSO"));

            securityConfig.UseAESHMMACPasswordEncryption(
                services,
                config.GetSection("settings")["DecryptionKey"]);
        });

        cCoder.Data.IServiceCollectionExtensions.AddCoreData(services, coreConnection);
        services.AddAppSecurityHostedServices();
        services.AddMailHostedServices();
        services.AddSchedulingHostedServices();
        services.AddWorkflowHostedServices();
        services.AddHttpContextAccessor();
        services.AddScoped(sp => CreateHttpContext(sp.GetService<IHttpContextAccessor>()?.HttpContext));
        services.AddScoped(sp => sp.GetRequiredService<HttpContext>().Request);
        services.AddScoped(sp => sp.GetRequiredService<HttpContext>().Session);
        services.AddSession();
        services.AddRouting();
        services.AddControllersWithViews();
        services.AddSignalR();
    }

    private static HttpContext CreateHttpContext(HttpContext httpContext)
    {
        if (httpContext is not null)
            return httpContext;

        DefaultHttpContext fallbackContext = new();
        fallbackContext.Features.Set<ISessionFeature>(new NoOpSessionFeature());
        return fallbackContext;
    }
}




