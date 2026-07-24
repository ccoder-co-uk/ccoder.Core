// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Orchestrations;

public interface IAppOrchestrationService
{
    ValueTask<App> AddAppAsync(App newApp);

    ValueTask<App> UpdateAppAsync(App updatedApp);

    ValueTask DeleteAppAsync(int appId);

    ValueTask<App> AddAsync(App app) =>
        AddAppAsync(newApp: app);

    ValueTask<App> UpdateAsync(App app) =>
        UpdateAppAsync(updatedApp: app);

    ValueTask DeleteAsync(int appId) =>
        DeleteAppAsync(appId: appId);
}