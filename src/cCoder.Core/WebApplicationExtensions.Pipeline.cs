// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.DocumentManagement;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Packaging;
using cCoder.Security;
using cCoder.Workflow;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static WebApplication UseCoreApi(
        this WebApplication app,
        ILogger log = null)
    {
        app.UseRouting();
        app.UseSession();
        app.UseCoreFormatters();
        app.StartSecurityWeb(log: log);
        app.UseAuthorization();
        app.UseCoreMetadataAuthorization();
        app.UseCoreApiDocumentation();
        app.StartMailWeb(log: log);
        app.StartDocumentManagementWeb(log: log);
        app.UsePackagingExposure(log: log);
        app.StartWorkflowWeb(log: log);
        app.StartContentManagementWeb(log: log);
        app.StartAppSecurityWeb(log: log);
        app.StartLoggingWeb(log: log);
        app.PopulateSecurityMetadataTypeCache();
        app.UseCoreDefaultCors();
        app.UseCoreExceptionHandling(errorHandler: HandleUnhandledException);
        app.UseCoreEventHandlers();
        app.UseCoreApiShell();
        return app;
    }
}
