// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;
using cCoder.Core.Models;
using cCoder.Data.Models.CMS;

using cCoder.Core.Services.Orchestrations;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class AppAggregationService(
    IContentManagementAppService contentManagementAppService,
    IAppSecurityAppService appSecurityAppService,
    IPlanningAppService planningAppService,
    IDocumentManagementAppService documentManagementAppService,
    IWorkflowAppService workflowAppService,
    IMailAppService mailAppService,
    IAppGraphEventService appGraphEventService,
    CoreConfiguration configuration
) : IAppAggregationService, IAppOrchestrationService
{
    public ValueTask<App> AddAppAsync(App newApp) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnAdd(newApp: newApp);

            App createdApp = await contentManagementAppService.AddAppAsync(
                newApp: newApp);

            App propagatedApp = MergeAppGraph(
                source: newApp,
                target: createdApp);

            await appGraphEventService.RaiseAppAddEventAsync(
                app: propagatedApp);

            await appSecurityAppService.AddAppAsync(newApp: propagatedApp);
            await planningAppService.AddAppAsync(newApp: propagatedApp);

            await documentManagementAppService.AddAppAsync(
                newApp: propagatedApp);

            await workflowAppService.AddAppAsync(newApp: propagatedApp);
            await mailAppService.AddAppAsync(newApp: propagatedApp);

            return propagatedApp;
        });

    public ValueTask<App> UpdateAppAsync(App updatedApp) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnUpdate(updatedApp: updatedApp);

            App persistedApp =
                await contentManagementAppService.UpdateAppAsync(
                    updatedApp: updatedApp);

            App propagatedApp = MergeAppGraph(
                source: updatedApp,
                target: persistedApp);

            await appGraphEventService.RaiseAppUpdateEventAsync(
                app: propagatedApp);

            await appSecurityAppService.UpdateAppAsync(
                updatedApp: propagatedApp);

            await planningAppService.UpdateAppAsync(
                updatedApp: propagatedApp);

            await documentManagementAppService.UpdateAppAsync(
                updatedApp: propagatedApp);

            await workflowAppService.UpdateAppAsync(
                updatedApp: propagatedApp);

            await mailAppService.UpdateAppAsync(updatedApp: propagatedApp);

            return propagatedApp;
        });

    public ValueTask<bool> DeleteAppAsync(int appId) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnDelete(appId: appId);

            if (HasExternalEventProvider(configuration: configuration))
            {
                await contentManagementAppService.DeleteAppAsync(
                    appId: appId);

                return true;
            }

            await planningAppService.DeleteAppAsync(appId: appId);
            await documentManagementAppService.DeleteAppAsync(appId: appId);
            await workflowAppService.DeleteAppAsync(appId: appId);
            await mailAppService.DeleteAppAsync(appId: appId);
            await contentManagementAppService.DeleteAppAsync(appId: appId);
            await appSecurityAppService.DeleteAppAsync(appId: appId);

            return false;
        });

    private static bool HasExternalEventProvider(
        CoreConfiguration configuration) =>
        string.Equals(
            a: configuration.Eventing.ProviderType,
            b: "Http",
            comparisonType: StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(
            value: configuration.Eventing.Http.HubUrl)
        || string.Equals(
            a: configuration.Eventing.ProviderType,
            b: "ServiceBus",
            comparisonType: StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(
            value: configuration.Eventing.ServiceBus.ConnectionString);

    private static App MergeAppGraph(App source, App target)
    {
        if (target == null)
        {
            return source;
        }

        if (source == null)
        {
            return target;
        }

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