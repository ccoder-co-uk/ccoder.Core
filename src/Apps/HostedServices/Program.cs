using cCoder.Core;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Eventing;
using cCoder.Eventing.Http.Models;
using cCoder.Eventing.Models;

namespace HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        ConfigureApplication(builder.Configuration);

        IConfiguration config = builder.Configuration;
        string ssoConnection = GetRequiredConnectionString(config, "SSO");
        string serviceBusConnection = config.GetConnectionString("ServiceBus");

        builder.Services.AddCoreHostedServices(coreBuilder =>
        {
            coreBuilder.WithCoreConfiguration(CreateCoreConfiguration(config));
            coreBuilder.WithSessionCache(ssoConnection);
            coreBuilder.WithSecurity(
                ssoConnection,
                GetRequiredSetting(config, "DecryptionKey"));
            coreBuilder.UseMSSQLProvider();
            coreBuilder.UseAppSecurity();
            coreBuilder.UseContentManagement();
            coreBuilder.UseDocumentManagement();
            coreBuilder.UseLogging();
            coreBuilder.UseMail();
            coreBuilder.UseScheduling();
            coreBuilder.UseWorkflow();
            coreBuilder.WithEventProviders(
                CreateExternalReceiveProvider<App>(["app_add", "app_update", "app_delete"]),
                CreateExternalReceiveProvider<Folder>(["folder_delete"]));
        });

        builder.Services.AddHostedEventTransport(
            serviceBusConnection,
            options => ConfigureHttpEventing(config, options));

        WebApplication app = builder.Build();
        app.StartCoreHostedServices();
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

    private static void ConfigureHttpEventing(
        IConfiguration configuration,
        HttpEventingOptions options)
    {
        if (int.TryParse(configuration["Eventing:Http:MaxConcurrency"], out int maxConcurrency))
            options.MaxConcurrency = maxConcurrency;
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
