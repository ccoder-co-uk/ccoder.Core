using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.Core.Cors;
using cCoder.DocumentManagement;
using cCoder.Mail;
using cCoder.Scheduling;
using cCoder.Workflow;
using EventLibrary.AzureServiceBus;
using EventLibrary.AzureServiceBus.Models;
using EventLibrary.Models;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication ListenToExternalEvents(this WebApplication app)
    {
        app.UseAppSecurityEventHandlers();
        app.UseAppSecurityDeleteEventHandlers();
        app.UseDocumentManagementEventHandlers();
        app.UseMailEventHandlers();
        app.UseSchedulingEventHandlers();
        app.UseWorkflowScheduledTaskExecutionHandlers();
        app.UseCoreInternalEventHandlers();
        app.UseConfiguredExternalEventProviders();
        return app;
    }

    private static WebApplication UseCoreEventHandlers(this WebApplication app)
    {
        app.UseAppSecurityEventHandlers();
        app.UseAppSecurityDeleteEventHandlers();
        app.ListenToContentManagementEvents();
        app.UseDocumentManagementEventHandlers();
        app.UseMailEventHandlers();
        app.UseSchedulingEventHandlers();
        app.UseWorkflowEventHandlers();
        app.UseCoreInternalEventHandlers();
        return app;
    }

    private static WebApplication UseCoreInternalEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        foreach (ICoreEventHandlers handlers in services.GetServices<ICoreEventHandlers>())
            handlers.ListenToAllEvents();

        return app;
    }

    private static WebApplication UseConfiguredExternalEventProviders(this WebApplication app)
    {
        IAzureServiceBusEventHub azureServiceBusEventHub =
            app.Services.GetService<IAzureServiceBusEventHub>();

        if (azureServiceBusEventHub is null)
            return app;

        foreach (EventProvider provider in app.Services.GetServices<EventProvider>())
            SubscribeConfiguredExternalEventProvider(azureServiceBusEventHub, provider);

        return app;
    }

    private static void SubscribeConfiguredExternalEventProvider(
        IAzureServiceBusEventHub azureServiceBusEventHub,
        EventProvider provider)
    {
        foreach (string eventName in (provider.Events ?? [])
                     .Where(eventName => !string.IsNullOrWhiteSpace(eventName))
                     .Distinct(StringComparer.Ordinal))
        {
            typeof(WebApplicationExtensions)
                .GetMethod(
                    nameof(SubscribeConfiguredExternalEventProviderCore),
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .MakeGenericMethod(provider.MessageType)
                .Invoke(null, [azureServiceBusEventHub, provider, eventName]);
        }
    }

    private static void SubscribeConfiguredExternalEventProviderCore<T>(
        IAzureServiceBusEventHub azureServiceBusEventHub,
        EventProvider provider,
        string eventName)
    {
        if (!provider.CanReceive<T>(eventName))
            return;

        azureServiceBusEventHub.ListenToEvent<T>(
            eventName,
            (serviceProvider, message) =>
                provider.HandleReceiveAsync(
                    serviceProvider,
                    eventName,
                    new EventMessage<T>
                    {
                        AuthInfo = new EventAuthInfo
                        {
                            SSOUserId =
                                serviceProvider.GetService<IServiceBusEventAuthInfo>()?.SSOUserId
                                ?? "Guest",
                        },
                        Data = message,
                    }));
    }
}
