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
using cCoder.Security;
using cCoder.Security.Objects.Events;
using cCoder.Workflow;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Workflow;
using AppSecurityAppOrchestrationService = cCoder.AppSecurity.Services.Orchestrations.IAppOrchestrationService;
using MailEventHandlerService = cCoder.Mail.Services.Foundations.Events.IEventHandlerService;
using CmsApp = cCoder.Data.Models.CMS.App;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication ListenToExternalEvents(this WebApplication app)
    {
        app.UseAppSecurityHostedServiceAddEventHandlers();
        app.StartContentManagementHostedServices();
        app.StartDocumentManagementHostedServices();
        app.StartLoggingHostedServices();
        app.StartMailHostedServices();
        app.StartWorkflowHostedServices();
        app.UseCoreEventHandlers();
        app.UseMailHostedServiceEventHandlers();
        app.UseHostedServicesServiceBusEventBridge();
        app.UseAppSecurityHostedServiceUpdateEventHandlers();
        app.UseAppSecurityHostedServiceDeleteEventHandlers();
        return app;
    }

    private static WebApplication UseCoreEventHandlers(this WebApplication app)
    {
        app.ListenToSecurityEvents();
        app.UseSecurityAccountEmailEventHandlers();
        app.UseServiceBusAppDeleteForwarder();
        return app;
    }

    private static WebApplication UseSecurityAccountEmailEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<SecurityAccountEvent, ISecurityAccountEmailOrchestrationService>(
name: SecurityAccountEventNames.RegistrationCreated, handler: static (service, accountEvent) => service.QueueRegistrationCreatedEmailAsync(accountEvent: accountEvent));
        eventHub.ListenToEvent<SecurityAccountEvent, ISecurityAccountEmailOrchestrationService>(
name: SecurityAccountEventNames.InvitationCreated, handler: static (service, accountEvent) => service.QueueInvitationCreatedEmailAsync(accountEvent: accountEvent));
        eventHub.ListenToEvent<SecurityAccountEvent, ISecurityAccountEmailOrchestrationService>(
name: SecurityAccountEventNames.PasswordResetRequested, handler: static (service, accountEvent) => service.QueuePasswordResetRequestedEmailAsync(accountEvent: accountEvent));

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
name: "app_delete", handler: static (service, entity) => service.ForwardAsync(app: entity));
        eventHub.ListenToEvent<Folder, ServiceBusFolderDeleteForwardingService>(
name: "folder_delete", handler: static (service, entity) => service.ForwardAsync(folder: entity));

        return app;
    }

    private static WebApplication UseAppSecurityHostedServiceAddEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<CmsApp, HostedServicesAppSecurityAppAddOrchestrationService>(
name: "app_add", handler: static (service, entity) => service.HandleAsync(app: entity));

        return app;
    }

    private static WebApplication UseAppSecurityHostedServiceUpdateEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<CmsApp, AppSecurityAppOrchestrationService>(
name: "app_update", handler: static (service, entity) => service.UpdateAppAsync(app: entity));

        return app;
    }

    private static WebApplication UseAppSecurityHostedServiceDeleteEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<CmsApp, AppSecurityAppOrchestrationService>(
name: "app_delete", handler: static (service, entity) => service.DeleteAsync(appId: entity.Id));

        return app;
    }

    private static WebApplication UseMailHostedServiceEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        foreach (MailEventHandlerService handlers in services.GetServices<MailEventHandlerService>())
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