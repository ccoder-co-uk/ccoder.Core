// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.DocumentManagement;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Packaging;
using cCoder.Workflow;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication UseCoreApi(
        this WebApplication app,
        ILogger log = null)
    {
        app.UseCoreApiDocumentation();
        app.UseCoreSecurityExposure(log: log);
        app.StartContentManagementWeb(onRequest: LogRequest, log: log);
        app.StartMailWeb(log: log);
        app.StartDocumentManagementWeb(log: log);
        app.UsePackagingExposure(log: log);
        app.StartWorkflowWeb(log);
        app.StartAppSecurityWeb(log: log);
        app.StartLoggingWeb(log: log);
        app.UseCoreDefaultCors();
        app.UseCoreExceptionHandling(errorHandler: HandleUnhandledException);
        app.UseCoreEventHandlers();
        app.UseCoreApiShell();
        return app;
    }
}