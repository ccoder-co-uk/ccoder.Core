using cCoder.Core;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using EventLibrary;
using EventLibrary.AzureServiceBus;
using EventLibrary.AzureServiceBus.Models;
using EventLibrary.Models;
using Web.Services.Setup;

namespace Web;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        ConfigureApplication(builder.Configuration);

        builder.Services.AddCoreWeb(coreconfiguration =>
        {
            coreconfiguration.WithEventProviders(
                CreateExternalEventProvider<App>(["app_add", "app_update", "app_delete"]),
                CreateExternalEventProvider<Folder>(["folder_delete"]));
        });
        builder.Services.AddScoped<IFirstTimeSetupStateService, FirstTimeSetupStateService>();
        builder.Services.AddScoped<FirstTimeSetupAssetService>();
        builder.Services.AddScoped<IFirstTimeSetupUserService, FirstTimeSetupUserService>();
        builder.Services.AddScoped<IFirstTimeSetupTenantService, FirstTimeSetupTenantService>();
        builder.Services.AddScoped<IFirstTimeSetupAppService, FirstTimeSetupAppService>();
        builder.Services.AddScoped<IFirstTimeSetupOrchestrationService, FirstTimeSetupOrchestrationService>();

        WebApplication app = builder.Build();
        app.StartCoreWeb();
        app.Run();
    }

    private static void ConfigureApplication(ConfigurationManager configuration)
    {
        configuration
            .AddEnvironmentVariables()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.testing.json", optional: true, reloadOnChange: true);
    }

    private static EventProvider<T> CreateExternalEventProvider<T>(string[] events) => new()
    {
        Events = events,
        SendHandler = async (serviceProvider, eventName, message) =>
        {
            IAzureServiceBusEventHub eventHub =
                serviceProvider.GetRequiredService<IAzureServiceBusEventHub>();

            await eventHub.RaiseEventAsync(
                eventName,
                new ServiceBusEventMessage<T>
                {
                    AuthInfo = new ServiceBusEventAuthInfo
                    {
                        SSOUserId = message.AuthInfo?.SSOUserId ?? "Guest",
                    },
                    Data = message.Data,
                });
        },
        ReceiveHandler = async (serviceProvider, eventName, message) =>
        {
            IEventHub eventHub =
                serviceProvider.GetRequiredService<IEventHub>();

            await eventHub.RaiseEventAsync(
                eventName,
                new EventMessage<T>
                {
                    AuthInfo = new EventAuthInfo
                    {
                        SSOUserId = message.AuthInfo?.SSOUserId ?? "Guest",
                    },
                    Data = message.Data,
                });
        }
    };
}
