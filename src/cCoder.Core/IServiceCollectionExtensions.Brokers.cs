using cCoder.Core.Brokers.AppSecurity;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Brokers.Mail;
using cCoder.Core.Brokers.Packaging;
using cCoder.Core.Brokers.Planning;
using cCoder.Core.Brokers.Workflow;
using cCoder.Packaging;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void AddCoreBrokers(IServiceCollection services)
    {
        services.AddTransient<IContentManagementAppBroker, ContentManagementAppBroker>();
        services.AddTransient<IHttpRequestBroker, HttpRequestBroker>();
        services.AddTransient<IAppSecurityAppBroker, AppSecurityAppBroker>();
        services.AddTransient<IPlanningAppBroker, PlanningAppBroker>();
        services.AddTransient<IDocumentManagementAppBroker, DocumentManagementAppBroker>();
        services.AddTransient<IWorkflowAppBroker, WorkflowAppBroker>();
        services.AddTransient<IMailAppBroker, MailAppBroker>();
        services.AddTransient<IMailManagerBroker, MailManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppDomainProvider, AppDomainProvider>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppSecurityPackageManagerBroker, AppSecurityPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker, ContentManagementPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IDocumentManagementPackageManagerBroker, DocumentManagementPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker, SchedulingPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker, WorkflowPackageManagerBroker>();
        services.AddPackaging();
    }
}
