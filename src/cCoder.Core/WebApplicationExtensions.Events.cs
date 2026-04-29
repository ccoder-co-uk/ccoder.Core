using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.Core.Cors;
using cCoder.DocumentManagement;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Scheduling;
using cCoder.Security;
using cCoder.Workflow;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication ListenToExternalEvents(this WebApplication app)
    {
        app.StartAppSecurityHostedServices();
        app.StartContentManagementHostedServices();
        app.StartDocumentManagementHostedServices();
        app.StartLoggingHostedServices();
        app.StartMailHostedServices();
        app.StartSchedulingHostedServices();
        app.StartWorkflowHostedServices();
        app.UseCoreInternalEventHandlers();
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
}
