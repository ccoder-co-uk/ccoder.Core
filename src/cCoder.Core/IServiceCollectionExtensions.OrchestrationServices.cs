// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Services.Aggregations;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void AddCoreOrchestrationServices(IServiceCollection services)
    {
        services.AddTransient<IAppAggregationService, AppAggregationService>();
        services.AddTransient<IAppOrchestrationService, AppAggregationService>();
        services.AddTransient<ITemplatedEmailOrchestrationService, TemplatedEmailOrchestrationService>();
        services.AddTransient<IUserRegistrationOrchestrationService, UserRegistrationOrchestrationService>();
        services.AddTransient<
            ISecurityAccountEmailAggregationService,
            SecurityAccountEmailAggregationService>();
    }
}
