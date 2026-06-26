using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.DocumentManagement;
using cCoder.Eventing;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Models;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Scheduling;
using cCoder.Security;
using cCoder.Workflow;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Workflow;
using AppSecurityAppOrchestrationService = cCoder.AppSecurity.Services.Orchestrations.IAppOrchestrationService;
using MailEventHandlerService = cCoder.Mail.Services.Foundations.Events.IEventHandlerService;
using SchedulingEventHandlerService = cCoder.Scheduling.Services.Foundations.Events.IEventHandlerService;
using WorkflowInstanceManagementOrchestrationService = cCoder.Workflow.Services.Orchestrations.IWorkflowInstanceManagementOrchestrationService;
using WorkflowEventHandlerService = cCoder.Workflow.Services.Foundations.Events.IEventHandlerService;
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
        app.StartSchedulingHostedServices();
        app.StartWorkflowHostedServices();
        app.UseMailHostedServiceEventHandlers();
        app.UseSchedulingHostedServiceEventHandlers();
        app.UseWorkflowHostedServiceEventHandlers();
        app.UseHostedServicesWorkflowExecutionEventHandlers();
        app.UseHostedServicesServiceBusEventBridge();
        app.UseAppSecurityHostedServiceUpdateEventHandlers();
        app.UseAppSecurityHostedServiceDeleteEventHandlers();
        return app;
    }

    private static WebApplication UseCoreEventHandlers(this WebApplication app)
    {
        app.ListenToSecurityEvents();
        app.UseWorkflowScheduledTaskExecuteEventHandlers();
        app.UseServiceBusAppDeleteForwarder();
        return app;
    }

    private static WebApplication UseWorkflowScheduledTaskExecuteEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        foreach (WorkflowEventHandlerService handlers in services.GetServices<WorkflowEventHandlerService>())
            handlers.ListenToScheduledTaskExecuteEvents();

        return app;
    }

    private static WebApplication UseServiceBusAppDeleteForwarder(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IAzureServiceBusEventHub serviceBusEventHub =
            scope.ServiceProvider.GetService<IAzureServiceBusEventHub>();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        if (serviceBusEventHub is null)
            return app;

        eventHub.ListenToEvent<CmsApp, ServiceBusAppDeleteForwardingService>(
            "app_delete",
            static (service, entity) => service.ForwardAsync(entity));
        eventHub.ListenToEvent<Folder, ServiceBusFolderDeleteForwardingService>(
            "folder_delete",
            static (service, entity) => service.ForwardAsync(entity));

        return app;
    }

    private static WebApplication UseAppSecurityHostedServiceAddEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<CmsApp, HostedServicesAppSecurityAppAddOrchestrationService>(
            "app_add",
            static (service, entity) => service.HandleAsync(entity));

        return app;
    }

    private static WebApplication UseAppSecurityHostedServiceUpdateEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<CmsApp, AppSecurityAppOrchestrationService>(
            "app_update",
            static (service, entity) => service.UpdateAsync(entity));
 
        return app;
    }

    private static WebApplication UseAppSecurityHostedServiceDeleteEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<CmsApp, AppSecurityAppOrchestrationService>(
            "app_delete",
            static (service, entity) => service.DeleteAsync(entity.Id));

        return app;
    }

    private static WebApplication UseMailHostedServiceEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        foreach (MailEventHandlerService handlers in services.GetServices<MailEventHandlerService>())
            handlers.ListenToAllEvents();

        return app;
    }

    private static WebApplication UseSchedulingHostedServiceEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        foreach (SchedulingEventHandlerService handlers in services.GetServices<SchedulingEventHandlerService>())
            handlers.ListenToAllEvents();

        return app;
    }

    private static WebApplication UseWorkflowHostedServiceEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        foreach (WorkflowEventHandlerService handlers in services.GetServices<WorkflowEventHandlerService>())
        {
            handlers.ListenToAllEvents();
            handlers.ListenToScheduledTaskExecuteEvents();
        }

        return app;
    }

    private static WebApplication UseHostedServicesWorkflowExecutionEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetService<IEventHub>();

        eventHub?.ListenToEvent<FlowInstanceData, WorkflowInstanceManagementOrchestrationService>(
            "flow_instance_data_add",
            static async (service, entity) =>
            {
                await ExecuteQueuedWorkflowInstanceAsync(service, entity);
            });

        return app;
    }

    private static WebApplication UseHostedServicesServiceBusEventBridge(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IAzureServiceBusEventHub eventHub = scope.ServiceProvider.GetService<IAzureServiceBusEventHub>();

        if (eventHub is null)
            return app;

        eventHub.ListenToLocalEventHub<CmsApp>("app_add");
        eventHub.ListenToLocalEventHub<CmsApp>("app_update");
        eventHub.ListenToLocalEventHub<CmsApp>("app_delete");
        eventHub.ListenToLocalEventHub<Folder>("folder_delete");
        eventHub.ListenToLocalEventHub<FlowInstanceData>("flow_instance_data_add");

        return app;
    }

    private static void ListenToLocalEventHub<T>(
        this IAzureServiceBusEventHub serviceBusEventHub,
        string eventName) =>
        serviceBusEventHub.ListenToEvent<T>(
            eventName,
            async (serviceProvider, entity) =>
            {
                IEventHub localEventHub = serviceProvider.GetRequiredService<IEventHub>();
                IServiceBusEventAuthInfo authInfo =
                    serviceProvider.GetService<IServiceBusEventAuthInfo>();

                await localEventHub.RaiseEventAsync(
                    eventName,
                    new EventMessage<T>
                    {
                        AuthInfo = new EventAuthInfo
                        {
                            SSOUserId = authInfo?.SSOUserId ?? string.Empty
                        },
                        Data = entity
                    });
            });

    private static async ValueTask ExecuteQueuedWorkflowInstanceAsync(
        WorkflowInstanceManagementOrchestrationService service,
        FlowInstanceData entity)
    {
        if (string.Equals(entity.State, "Queued", StringComparison.OrdinalIgnoreCase))
            await service.ExecuteWaitingQueuedInstanceByIdAsync(entity.Id);
    }
}
