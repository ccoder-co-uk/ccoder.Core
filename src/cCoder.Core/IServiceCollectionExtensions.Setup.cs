using Web.Services.Setup;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    public static void AddCoreFirstTimeSetup(this IServiceCollection services)
    {
        services.AddScoped<IFirstTimeSetupStateService, FirstTimeSetupStateService>();
        services.AddScoped<FirstTimeSetupAssetService>();
        services.AddScoped<IFirstTimeSetupUserService, FirstTimeSetupUserService>();
        services.AddScoped<IFirstTimeSetupTenantService, FirstTimeSetupTenantService>();
        services.AddScoped<IFirstTimeSetupAppService, FirstTimeSetupAppService>();
        services.AddScoped<IFirstTimeSetupOrchestrationService, FirstTimeSetupOrchestrationService>();
    }
}
