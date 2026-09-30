// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Data.Models.CMS;
using cCoder.Core.Services.Aggregations;

namespace cCoder.Core.Services.Orchestrations;

internal interface IAppOrchestrationService : IAppAggregationService
{
    ValueTask<App> AddAsync(App app) =>
        AddAppAsync(newApp: app);

    ValueTask<App> UpdateAsync(App app) =>
        UpdateAppAsync(updatedApp: app);

    ValueTask<bool> DeleteAsync(int appId) =>
        DeleteAppAsync(appId: appId);
}