using cCoder.Core;
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
        IConfiguration config = ConfigureApplication(builder.Configuration, builder.Environment);

        builder.Services.AddCoreWeb(coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(coreConfig =>
            {
                coreConfig.CoreConnectionString = config.GetValue<string>("ConnectionStrings:Core");
                coreConfig.SecurityConnectionString = config.GetValue<string>("ConnectionStrings:SSO");
                coreConfig.DecryptionKey = config.GetValue<string>("Settings:DecryptionKey");
                coreConfig.CacheSource = config.GetValue<string>("Settings:CacheSource");
                coreConfig.CacheSourceAppId = config.GetValue<int?>("Settings:CacheSourceAppId");
                coreConfig.CacheExpiry = config.GetValue<int?>("Settings:CacheExpiry");
                coreConfig.SslPort = config.GetValue<int?>("Settings:sslPort");
                coreConfig.WorkflowServiceUrl = config.GetValue<string>("Services:Workflow");
                coreConfig.HttpEventHubUrl = GetHttpEventHubUrl(config);
                coreConfig.EnableHttpEventing = !string.IsNullOrWhiteSpace(coreConfig.HttpEventHubUrl);
                coreConfig.MaxConcurrency = config.GetValue<int?>("Eventing:Http:MaxConcurrency") ?? 1;
                coreConfig.DebugInfo = config.GetValue<bool>("DebugInfo");
                coreConfig.LogSQL = config.GetValue<bool>("LogSQL");

                if (coreConfig.EnableHttpEventing)
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

    private static IConfiguration ConfigureApplication(ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.testing.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return configuration;
    }

    private static string GetHttpEventHubUrl(IConfiguration configuration)
    {
        string explicitHubUrl = configuration.GetValue<string>("Eventing:Http:HubUrl");

        if (!string.IsNullOrWhiteSpace(explicitHubUrl))
            return explicitHubUrl;

        if (!(configuration.GetValue<bool?>("Settings:enableExternalEventing") ?? true))
            return string.Empty;

        string hostedServicesRoot = configuration.GetValue<string>("Services:HostedServices");

        return string.IsNullOrWhiteSpace(hostedServicesRoot)
            ? null
            : $"{hostedServicesRoot.TrimEnd('/')}/Api/Eventing";
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
