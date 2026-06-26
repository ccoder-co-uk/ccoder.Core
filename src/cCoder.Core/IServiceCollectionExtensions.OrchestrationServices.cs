using cCoder.Core.Services.Orchestrations;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void AddCoreOrchestrationServices(IServiceCollection services)
    {
        services.AddTransient<IAppOrchestrationService, AppOrchestrationService>();
        services.AddTransient<ITemplatedEmailOrchestrationService, TemplatedEmailOrchestrationService>();
        services.AddTransient<ICMSUserRegistrationOrchestrationService, CMSUserRegistrationOrchestrationService>();
        services.AddTransient<IUserRegistrationOrchestrationService, UserRegistrationOrchestrationService>();
        services.AddTransient<IUserPasswordOrchestrationService, UserPasswordOrchestrationService>();
    }
}
