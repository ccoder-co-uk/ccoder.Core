using cCoder.Core;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Http;
using cCoder.Eventing.Http.Models;
using cCoder.Eventing.Models;

namespace Web;

public class Program
{
    private const string DefaultHttpEventHubPath = "Api/Eventing/Http";

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        ConfigureApplication(builder.Configuration);

        IConfiguration config = builder.Configuration;
        string ssoConnection = GetRequiredConnectionString(config, "SSO");
        string serviceBusConnection = config.GetConnectionString("ServiceBus");
        string httpEventHubUrl = GetHttpEventHubUrl(config);

        builder.Services.AddCoreWeb(coreBuilder =>
        {
            coreBuilder.WithCoreConfiguration(CreateCoreConfiguration(config));
            coreBuilder.WithSessionCache(ssoConnection);
            coreBuilder.WithSecurity(
                ssoConnection,
                GetRequiredSetting(config, "DecryptionKey"));
            coreBuilder.AddAppSecurityApi();
            coreBuilder.AddContentManagementApi();
            coreBuilder.AddDocumentManagementApi();
            coreBuilder.AddLoggingApi();
            coreBuilder.AddMailApi();
            coreBuilder.AddSchedulingApi();
            coreBuilder.AddWorkflowApi();
            coreBuilder.UseLegacyCoreContext();

            if (HasExternalEventTransport(serviceBusConnection, httpEventHubUrl))
            {
                coreBuilder.WithEventProviders(
                    CreateExternalSendProvider<App>(["app_add", "app_update", "app_delete"]),
                    CreateExternalSendProvider<Folder>(["folder_delete"]));
            }
        });

        builder.Services.AddExternalEventTransport(
            serviceBusConnection,
            httpEventHubUrl,
            options => ConfigureHttpEventing(config, options));
        builder.Services.AddCoreFirstTimeSetup();

        WebApplication app = builder.Build();
        app.StartCoreWeb();
        app.Run();
    }

    private static void ConfigureApplication(ConfigurationManager configuration)
    {
        configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.testing.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    private static Config CreateCoreConfiguration(IConfiguration configuration) =>
        new()
        {
            ConnectionStrings = ReadSection(configuration, "ConnectionStrings"),
            Settings = ReadSection(configuration, "Settings"),
            Services = ReadSection(configuration, "Services"),
            DebugInfo = configuration.GetValue<bool>("DebugInfo"),
            LogSQL = configuration.GetValue<bool>("LogSQL"),
        };

    private static Dictionary<string, string> ReadSection(
        IConfiguration configuration,
        string sectionName) =>
        configuration
            .GetSection(sectionName)
            .GetChildren()
            .Where(child => child.Value is not null)
            .ToDictionary(child => child.Key, child => child.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

    private static string GetRequiredConnectionString(
        IConfiguration configuration,
        string name) =>
        configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"ConnectionStrings:{name} must be configured.");

    private static string GetRequiredSetting(
        IConfiguration configuration,
        string name) =>
        configuration.GetSection("Settings")[name]
            ?? throw new InvalidOperationException($"Settings:{name} must be configured.");

    private static bool HasExternalEventTransport(
        string serviceBusConnectionString,
        string httpEventHubUrl) =>
        !string.IsNullOrWhiteSpace(serviceBusConnectionString)
        || !string.IsNullOrWhiteSpace(httpEventHubUrl);

    private static string GetHttpEventHubUrl(IConfiguration configuration)
    {
        string configuredHubUrl = configuration["Eventing:Http:HubUrl"];

        if (!string.IsNullOrWhiteSpace(configuredHubUrl))
            return NormalizeHttpEventHubUrl(configuredHubUrl);

        string hostedServicesRoot = configuration["Services:HostedServices"];

        return string.IsNullOrWhiteSpace(hostedServicesRoot)
            ? null
            : NormalizeHttpEventHubUrl(hostedServicesRoot);
    }

    private static string NormalizeHttpEventHubUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            return value;

        string path = uri.AbsolutePath?.Trim('/') ?? string.Empty;

        if (string.Equals(path, DefaultHttpEventHubPath, StringComparison.OrdinalIgnoreCase))
            return uri.ToString();

        if (string.IsNullOrWhiteSpace(path))
            return $"{value.TrimEnd('/')}/{DefaultHttpEventHubPath}";

        return uri.ToString();
    }

    private static void ConfigureHttpEventing(
        IConfiguration configuration,
        HttpEventingOptions options)
    {
        if (int.TryParse(configuration["Eventing:Http:MaxConcurrency"], out int maxConcurrency))
            options.MaxConcurrency = maxConcurrency;
    }

    private static EventProvider<T> CreateExternalSendProvider<T>(string[] eventNames) =>
        new()
        {
            Events = eventNames,
            SendHandler = async (serviceProvider, eventName, message) =>
            {
                IAzureServiceBusEventHub azureServiceBusEventHub =
                    serviceProvider.GetService<IAzureServiceBusEventHub>();

                if (azureServiceBusEventHub is not null)
                {
                    await azureServiceBusEventHub.RaiseEventAsync(
                        eventName,
                        new ServiceBusEventMessage<T>
                        {
                            AuthInfo = new ServiceBusEventAuthInfo
                            {
                                SSOUserId = message.AuthInfo?.SSOUserId ?? "Guest",
                            },
                            Data = message.Data,
                        });

                    return;
                }

                IHttpEventHub httpEventHub = serviceProvider.GetService<IHttpEventHub>();

                if (httpEventHub is not null)
                {
                    await httpEventHub.RaiseEventAsync(eventName, message);
                    return;
                }

                throw new InvalidOperationException(
                    "No external event transport has been registered for the configured event provider.");
            }
        };
}
