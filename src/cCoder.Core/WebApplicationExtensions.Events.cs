// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.DocumentManagement;
using cCoder.Eventing;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Models;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Mail.Exposures.EventHandlers;
using cCoder.Core.Services.Aggregations;
using cCoder.Security;
using cCoder.Security.Models.Events;
using cCoder.Workflow;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Workflow;
using CmsApp = cCoder.Data.Models.CMS.App;
using PackagingPackageImportEvent = cCoder.Packaging.Models.PackageImportEvent;
using PackageImportAggregation = cCoder.Core.Services.Aggregations.Packages.IPackageImportAggregationService;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication ListenToExternalEvents(this WebApplication app)
    {
        app.StartContentManagementHostedServices();
        app.StartAppSecurityHostedServices();
        app.StartDocumentManagementHostedServices();
        app.StartLoggingHostedServices();
        app.StartMailHostedServices();
        app.StartWorkflowHostedServices();
        app.UseCoreEventHandlers();
        app.UseMailHostedServiceEventHandlers();
        app.UseHostedServicesServiceBusEventBridge();
        app.StartContentManagementFinalAppDeleteEventHandler();
        return app;
    }

    private static WebApplication UseCoreEventHandlers(this WebApplication app)
    {
        app.ListenToSecurityEvents();
        app.UsePackageImportEventHandler();
        app.UseSecurityAccountEmailEventHandlers();
        app.UseServiceBusAppDeleteForwarder();
        return app;
    }

    private static WebApplication UsePackageImportEventHandler(
        this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<
            PackagingPackageImportEvent,
            PackageImportAggregation>(
                name: "package_import",
                handler: static (service, packageImportEvent) =>
                    service.ProcessPackageImportEventAsync(
                        packageImportEvent: packageImportEvent));

        return app;
    }

    private static WebApplication UseSecurityAccountEmailEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<SecurityAccountEvent, ISecurityAccountEmailAggregationService>(
name: SecurityAccountEventKind.RegistrationCreated.ToEventName(), handler: static (service, accountEvent) => service.QueueRegistrationCreatedSecurityAccountEventEmailAsync(accountEvent: accountEvent));

        eventHub.ListenToEvent<SecurityAccountEvent, ISecurityAccountEmailAggregationService>(
name: SecurityAccountEventKind.InvitationCreated.ToEventName(), handler: static (service, accountEvent) => service.QueueInvitationCreatedSecurityAccountEventEmailAsync(accountEvent: accountEvent));

        eventHub.ListenToEvent<SecurityAccountEvent, ISecurityAccountEmailAggregationService>(
name: SecurityAccountEventKind.PasswordResetRequested.ToEventName(), handler: static (service, accountEvent) => service.QueuePasswordResetRequestedSecurityAccountEventEmailAsync(accountEvent: accountEvent));

        return app;
    }

    private static WebApplication UseServiceBusAppDeleteForwarder(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        IAzureServiceBusEventHub serviceBusEventHub =
            scope.ServiceProvider.GetService<IAzureServiceBusEventHub>();

        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        if (serviceBusEventHub is null)
        {
            return app;
        }

        eventHub.ListenToEvent<CmsApp, ServiceBusAppDeleteForwardingService>(
name: "app_delete", handler: static (service, entity) => service.ForwardAppDeleteAsync(app: entity));

        eventHub.ListenToEvent<Folder, ServiceBusFolderDeleteForwardingService>(
name: "folder_delete", handler: static (service, entity) => service.ForwardFolderDeleteAsync(folder: entity));

        return app;
    }

    private static WebApplication UseMailHostedServiceEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        foreach (IMailEventHandlers handlers in services.GetServices<IMailEventHandlers>())
        {
            handlers.ListenToAllEvents();
        }

        return app;
    }

    private static WebApplication UseHostedServicesServiceBusEventBridge(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IAzureServiceBusEventHub eventHub = scope.ServiceProvider.GetService<IAzureServiceBusEventHub>();

        if (eventHub is null)
        {
            return app;
        }

        eventHub.ListenToLocalEventHub<CmsApp>(eventName: "app_add");
        eventHub.ListenToLocalEventHub<CmsApp>(eventName: "app_update");
        eventHub.ListenToLocalEventHub<CmsApp>(eventName: "app_delete");
        eventHub.ListenToLocalEventHub<Folder>(eventName: "folder_delete");
        eventHub.ListenToLocalEventHub<ScheduledTask>(
            eventName: "scheduled_task_execute");
        eventHub.ListenToLocalEventHub<FlowInstanceData>(eventName: "flow_instance_data_add");

        return app;
    }

    private static void ListenToLocalEventHub<T>(
        this IAzureServiceBusEventHub serviceBusEventHub,
        string eventName) =>
        serviceBusEventHub.ListenToEvent<T>(
name: eventName, handler: async (serviceProvider, entity) =>
            {
                IEventHub localEventHub = serviceProvider.GetRequiredService<IEventHub>();

                IServiceBusEventAuthInfo authInfo =
                    serviceProvider.GetService<IServiceBusEventAuthInfo>();

                await localEventHub.RaiseEventAsync(
name: eventName, message: new EventMessage<T>
{
    AuthInfo = new EventAuthInfo
    {
        SSOUserId = authInfo?.SSOUserId ?? string.Empty
    },
    Data = entity
});
            });

}
