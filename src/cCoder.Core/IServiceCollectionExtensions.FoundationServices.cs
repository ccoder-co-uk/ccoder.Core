using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void AddCoreFoundationServices(IServiceCollection services)
    {
        services.AddTransient<IContentManagementAppService, ContentManagementAppService>();
        services.AddTransient<IAllowedOriginStoreService, AllowedOriginStoreService>();
        services.AddTransient<IAppSecurityAppService, AppSecurityAppService>();
        services.AddTransient<IPlanningAppService, PlanningAppService>();
        services.AddTransient<IDocumentManagementAppService, DocumentManagementAppService>();
        services.AddTransient<IWorkflowAppService, WorkflowAppService>();
        services.AddTransient<IMailAppService, MailAppService>();
        services.AddTransient<IMailManagerService, MailManagerService>();
        services.AddTransient<ServiceBusAppDeleteForwardingService>();
        services.AddTransient<ServiceBusFolderDeleteForwardingService>();
    }
}
