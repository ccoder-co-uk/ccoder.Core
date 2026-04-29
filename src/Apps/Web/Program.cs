using cCoder.Core;
using cCoder.Core.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Eventing.Http;
using cCoder.Eventing.Models;

namespace Web;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IConfiguration config = ConfigureApplication(builder.Configuration);
        CoreHostConfiguration hostConfiguration = CoreHostConfigurationReader.ReadForWeb(config);

        builder.Services.AddCoreWeb(coreBuilder =>
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
                coreConfig.HttpEventHubUrl = hostConfiguration.HttpEventHubUrl;
                coreConfig.EnableHttpEventing = hostConfiguration.EnableHttpEventing;
                coreConfig.MaxConcurrency = hostConfiguration.MaxConcurrency;
                coreConfig.DebugInfo = hostConfiguration.DebugInfo;
                coreConfig.LogSQL = hostConfiguration.LogSQL;

                if (hostConfiguration.EnableHttpEventing)
                {
                    coreConfig.EventProviders =
                    [
                        CreateExternalSendProvider<App>(["app_add", "app_update", "app_delete"]),
                        CreateExternalSendProvider<Folder>(["folder_delete"])
                    ];
                }
            });
        });

        WebApplication app = builder.Build();
        app.StartCoreWeb();
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

    private static EventProvider<T> CreateExternalSendProvider<T>(string[] eventNames) =>
        new()
        {
            Events = eventNames,
            SendHandler = async (serviceProvider, eventName, message) =>
            {
                IHttpEventHub httpEventHub = serviceProvider.GetRequiredService<IHttpEventHub>();
                await httpEventHub.RaiseEventAsync(eventName, message);
            }
        };
}
