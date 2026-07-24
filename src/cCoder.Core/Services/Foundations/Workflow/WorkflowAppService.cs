// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Workflow;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Workflow;

internal sealed partial class WorkflowAppService(IWorkflowAppBroker workflowAppBroker)
    : IWorkflowAppService
{
    public ValueTask AddAppAsync(App newApp) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnAdd(newApp: newApp);

            App flatApp = CreateFlatApp(app: newApp);

            await workflowAppBroker.AddAppAsync(newApp: flatApp);
        });

    public ValueTask UpdateAppAsync(App updatedApp) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnUpdate(updatedApp: updatedApp);

            App flatApp = CreateFlatApp(app: updatedApp);

            await workflowAppBroker.UpdateAppAsync(updatedApp: flatApp);
        });

    public ValueTask DeleteAsync(int appId) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnDelete(appId: appId);

            await workflowAppBroker.DeleteAsync(appId: appId);
        });

    private static App CreateFlatApp(App app) =>
        new()
        {
            Id = app.Id,
            DefaultCultureId = app.DefaultCultureId,
            TenantId = app.TenantId,
            Name = app.Name,
            Domain = app.Domain,
            DefaultTheme = app.DefaultTheme,
            ConfigJson = app.ConfigJson,
        };
}