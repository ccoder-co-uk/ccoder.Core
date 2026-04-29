using cCoder.Core;
using cCoder.Core.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Eventing;
using cCoder.Eventing.Models;

namespace HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IConfiguration config = ConfigureApplication(builder.Configuration);
        CoreHostConfiguration hostConfiguration = CoreHostConfigurationReader.ReadForHostedServices(config);

        builder.Services.AddCoreHostedServices(coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(coreConfig =>
            {
                coreConfig.CoreConnectionString = hostConfiguration.CoreConnectionString;
                coreConfig.SecurityConnectionString = hostConfiguration.SecurityConnectionString;
                coreConfig.DecryptionKey = hostConfiguration.DecryptionKey;
                coreConfig.CacheSource = hostConfiguration.CacheSource;
                coreConfig.CacheSourceAppId = hostConfiguration.CacheSourceAppId;
                coreConfig.CacheExpiry = hostConfiguration.CacheExpiry;
                coreConfig.SslPort = hostConfiguration.SslPort;
                coreConfig.WorkflowServiceUrl = hostConfiguration.WorkflowServiceUrl;
                coreConfig.MaxConcurrency = hostConfiguration.MaxConcurrency;
                coreConfig.DebugInfo = hostConfiguration.DebugInfo;
                coreConfig.LogSQL = hostConfiguration.LogSQL;
                coreConfig.EventProviders =
                [
                    CreateExternalReceiveProvider<App>(["app_add", "app_update", "app_delete"]),
                    CreateExternalReceiveProvider<Folder>(["folder_delete"])
                ];
            });
        });

        WebApplication app = builder.Build();
        app.StartCoreHostedServices();
        app.Run();
    }

    private static IConfiguration ConfigureApplication(ConfigurationManager configuration)
    {
        configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.testing.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return configuration;
    }

    private static EventProvider<T> CreateExternalReceiveProvider<T>(string[] eventNames) =>
        new()
        {
            Events = eventNames,
            ReceiveHandler = async (serviceProvider, eventName, message) =>
            {
                IEventHub eventHub = serviceProvider.GetRequiredService<IEventHub>();

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
