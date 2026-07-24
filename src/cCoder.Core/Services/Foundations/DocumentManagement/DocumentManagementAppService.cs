// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.DocumentManagement;

internal sealed partial class DocumentManagementAppService(
    IDocumentManagementAppBroker documentManagementAppBroker)
    : IDocumentManagementAppService
{
    public ValueTask AddAppAsync(App newApp) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnAdd(newApp: newApp);

            App flatApp = CreateFlatApp(app: newApp);

            await documentManagementAppBroker.AddAppAsync(newApp: flatApp);
        });

    public ValueTask UpdateAppAsync(App updatedApp) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnUpdate(updatedApp: updatedApp);

            App flatApp = CreateFlatApp(app: updatedApp);

            await documentManagementAppBroker.UpdateAppAsync(updatedApp: flatApp);
        });

    public ValueTask DeleteAsync(int appId) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnDelete(appId: appId);

            await documentManagementAppBroker.DeleteAsync(appId: appId);
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