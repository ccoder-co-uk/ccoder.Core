using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.DocumentManagement;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Packaging;
using cCoder.Scheduling;
using cCoder.Workflow;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication UseCoreApi(
        this WebApplication app,
        ILogger log = null)
    {
        app.UseCoreApiDocumentation();
        app.UseCoreSecurityExposure(log);
        app.StartAppSecurityWeb(log);
        app.StartContentManagementWeb(LogRequest, log);
        app.StartMailWeb(log);
        app.StartDocumentManagementWeb(log);
        app.UsePackagingExposure(log);
        app.StartSchedulingWeb(log);
        app.StartWorkflowWeb(log);
        app.StartLoggingWeb(log);
        app.UseCoreDefaultCors();
        app.UseCoreExceptionHandling(HandleUnhandledException);
        app.UseCoreEventHandlers();
        app.UseCoreApiShell();
        return app;
    }
}
