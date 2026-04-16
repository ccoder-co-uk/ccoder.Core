using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using EventLibrary;
using EventLibrary.AzureServiceBus;
using EventLibrary.AzureServiceBus.Models;
using EventLibrary.Models;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    internal static void AddCoreApiEventing(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<EventProvider> eventProviders
    )
    {
        string serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");

        bool enableExternalEventing =
            configuration.GetValue<bool>("Settings:enableExternalEventing") &&
            !string.IsNullOrWhiteSpace(serviceBusConnectionString);

        EventProvider[] configuredEventProviders = NormalizeEventProviders(eventProviders);

        if (enableExternalEventing)
        {
            services.AddAzureServiceBusEventing(serviceBusConnectionString);
            services.AddEventProviders(configuredEventProviders);
        }

        services.AddEventing();
    }

    internal static void AddCoreHostedEventing(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<EventProvider> eventProviders
    )
    {
        services.AddEventing();
        AddHostedWorkflowTriggerEventingTypes(services);
        services.AddEventProviders(NormalizeEventProviders(eventProviders));

        string serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");

        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
            return;

        services.AddAzureServiceBusEventing(serviceBusConnectionString);
    }

    private static EventProvider[] NormalizeEventProviders(IEnumerable<EventProvider> eventProviders)
    {
        EventProvider[] providers = (eventProviders ?? []).Where(provider => provider is not null).ToArray();

        return providers.Length > 0
            ? providers
            : CreateDefaultExternalEventProviders();
    }

    private static EventProvider[] CreateDefaultExternalEventProviders() =>
    [
        CreateExternalEventProvider<App>(["app_add", "app_update", "app_delete"]),
        CreateExternalEventProvider<Folder>(["folder_delete"]),
    ];

    private static EventProvider<T> CreateExternalEventProvider<T>(string[] eventNames) =>
        new()
        {
            Events = eventNames,
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

    private static void AddHostedWorkflowTriggerEventingTypes(IServiceCollection services)
    {
        services.AddEventingForType<App>();
        services.AddEventingForType<AppCulture>();
        services.AddEventingForType<cCoder.Data.Models.CommonObject>();
        services.AddEventingForType<Component>();
        services.AddEventingForType<Content>();
        services.AddEventingForType<Culture>();
        services.AddEventingForType<Layout>();
        services.AddEventingForType<Page>();
        services.AddEventingForType<PageInfo>();
        services.AddEventingForType<cCoder.Data.Models.Security.PageRole>();
        services.AddEventingForType<Resource>();
        services.AddEventingForType<Script>();
        services.AddEventingForType<Submission>();
        services.AddEventingForType<Template>();
        services.AddEventingForType<cCoder.Data.Models.Packaging.Package>();
        services.AddEventingForType<(int, cCoder.Data.Models.Packaging.Package)>();
        services.AddEventingForType<cCoder.Data.Models.Packaging.PackageItem>();
    }
}
