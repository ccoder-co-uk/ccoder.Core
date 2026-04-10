using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;

namespace cCoder.Core.Services.Orchestrations;

internal class AppOrchestrationService(
    IContentManagementAppService contentManagementAppService,
    IAppSecurityAppService appSecurityAppService,
    IPlanningAppService planningAppService,
    IDocumentManagementAppService documentManagementAppService,
    IWorkflowAppService workflowAppService,
    IMailAppService mailAppService
) : IAppOrchestrationService
{
    public async ValueTask<App> AddAsync(App app)
    {
        App createdApp = await contentManagementAppService.AddAsync(app);
        App propagatedApp = MergeAppGraph(app, createdApp);

        await appSecurityAppService.AddAsync(propagatedApp);
        await planningAppService.AddAsync(propagatedApp);
        await documentManagementAppService.AddAsync(propagatedApp);
        await workflowAppService.AddAsync(propagatedApp);
        await mailAppService.AddAsync(propagatedApp);
        return propagatedApp;
    }

    public async ValueTask<App> UpdateAsync(App app)
    {
        App updatedApp = await contentManagementAppService.UpdateAsync(app);
        App propagatedApp = MergeAppGraph(app, updatedApp);

        await appSecurityAppService.UpdateAsync(propagatedApp);
        await planningAppService.UpdateAsync(propagatedApp);
        await documentManagementAppService.UpdateAsync(propagatedApp);
        await workflowAppService.UpdateAsync(propagatedApp);
        await mailAppService.UpdateAsync(propagatedApp);
        return propagatedApp;
    }

    public async ValueTask DeleteAsync(int appId)
    {
        await planningAppService.DeleteAsync(appId);
        await documentManagementAppService.DeleteAsync(appId);
        await workflowAppService.DeleteAsync(appId);
        await mailAppService.DeleteAsync(appId);
        await contentManagementAppService.DeleteAsync(appId);
        await appSecurityAppService.DeleteAsync(appId);
    }

    private static App MergeAppGraph(App source, App target)
    {
        if (target == null)
            return source;

        if (source == null)
            return target;

        target.DefaultCultureId = source.DefaultCultureId ?? target.DefaultCultureId;
        target.TenantId = source.TenantId ?? target.TenantId;
        target.Name = source.Name ?? target.Name;
        target.Domain = source.Domain ?? target.Domain;
        target.DefaultTheme = source.DefaultTheme ?? target.DefaultTheme;
        target.ConfigJson = source.ConfigJson ?? target.ConfigJson;
        target.Cultures = source.Cultures ?? target.Cultures;
        target.Pages = source.Pages ?? target.Pages;
        target.Components = source.Components ?? target.Components;
        target.Scripts = source.Scripts ?? target.Scripts;
        target.Roles = source.Roles ?? target.Roles;
        target.Templates = source.Templates ?? target.Templates;
        target.Resources = source.Resources ?? target.Resources;
        target.Tasks = source.Tasks ?? target.Tasks;
        target.Calendars = source.Calendars ?? target.Calendars;
        target.Folders = source.Folders ?? target.Folders;
        target.Layouts = source.Layouts ?? target.Layouts;
        target.Flows = source.Flows ?? target.Flows;
        target.MailServers = source.MailServers ?? target.MailServers;
        target.MailQueue = source.MailQueue ?? target.MailQueue;
        target.SentMail = source.SentMail ?? target.SentMail;
        return target;
    }
}