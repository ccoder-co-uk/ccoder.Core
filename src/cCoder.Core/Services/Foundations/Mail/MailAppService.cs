// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Mail;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Mail;

internal sealed partial class MailAppService(IMailAppBroker mailAppBroker)
    : IMailAppService
{
    public ValueTask AddAppAsync(App newApp) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnAdd(newApp: newApp);

            App flatApp = CreateFlatApp(app: newApp);

            await mailAppBroker.AddAppAsync(newApp: flatApp);
        });

    public ValueTask UpdateAppAsync(App updatedApp) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnUpdate(updatedApp: updatedApp);

            App flatApp = CreateFlatApp(app: updatedApp);

            await mailAppBroker.UpdateAppAsync(updatedApp: flatApp);
        });

    public ValueTask DeleteAppAsync(int appId) =>
        TryCatch(operation: async ValueTask () =>
        {
            ValidateAppOnDelete(appId: appId);

            await mailAppBroker.DeleteAppAsync(appId: appId);
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