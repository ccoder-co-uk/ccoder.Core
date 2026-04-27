using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.Http;
using cCoder.Eventing.Http.Models;
using System.Text.Json.Serialization;
using Web.Services.Setup;

namespace Web;

internal static class ServiceCollectionExtensions
{
    public static bool AddExternalEventTransport(
        this IServiceCollection services,
        string serviceBusConnectionString,
        string httpEventHubUrl,
        Action<HttpEventingOptions> configureHttp = null)
    {
        if (!string.IsNullOrWhiteSpace(serviceBusConnectionString))
        {
            services.AddAzureServiceBusEventing(serviceBusConnectionString);
            return true;
        }

        if (string.IsNullOrWhiteSpace(httpEventHubUrl))
            return false;

        services.AddHttpEventing(options =>
        {
            options.HubUrl = httpEventHubUrl;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            configureHttp?.Invoke(options);
        });

        return true;
    }

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
