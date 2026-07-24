// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Planning;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Planning;

internal class PlanningAppService(IPlanningAppBroker planningAppBroker) : IPlanningAppService
{
    public ValueTask AddAsync(App app) =>
        planningAppBroker.AddAsync(app: app);

    public ValueTask UpdateAsync(App app) =>
        planningAppBroker.UpdateAsync(app: app);

    public ValueTask DeleteAsync(int appId) =>
        planningAppBroker.DeleteAsync(appId: appId);
}