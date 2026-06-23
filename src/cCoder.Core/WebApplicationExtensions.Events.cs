using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.Core.Cors;
using cCoder.DocumentManagement;
using cCoder.Eventing;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Scheduling;
using cCoder.Security;
using cCoder.Workflow;
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
        app.UseCoreInternalEventHandlers();
        app.UseAppSecurityHostedServiceUpdateEventHandlers();
        app.UseAppSecurityHostedServiceDeleteEventHandlers();
        return app;
    }

    private static WebApplication UseCoreEventHandlers(this WebApplication app)
    {
        app.ListenToSecurityEvents();
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
            handlers.ListenToAllEvents();

        return app;
    }

    private static WebApplication UseHostedServicesWorkflowExecutionEventHandlers(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IEventHub eventHub = scope.ServiceProvider.GetRequiredService<IEventHub>();

        eventHub.ListenToEvent<FlowInstanceData, WorkflowInstanceManagementOrchestrationService>(
            "flow_instance_data_add",
            static (service, entity) => service.ExecuteWaitingQueuedInstanceByIdAsync(entity.FlowDefinitionId));

        return app;
    }
}
