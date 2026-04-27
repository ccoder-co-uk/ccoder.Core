using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.Http;
using cCoder.Eventing.Http.Models;
using System.Text.Json.Serialization;

namespace HostedServices;

internal static class ServiceCollectionExtensions
{
    public static bool AddHostedEventTransport(
        this IServiceCollection services,
        string serviceBusConnectionString,
        Action<HttpEventingOptions> configureHttp = null)
    {
        bool hasTransport = false;

        if (!string.IsNullOrWhiteSpace(serviceBusConnectionString))
        {
            services.AddAzureServiceBusEventing(serviceBusConnectionString);
            hasTransport = true;
        }

        if (configureHttp is not null)
        {
            services.AddHttpEventing(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                configureHttp(options);
            });
            services.AddControllers().AddHttpEventingControllers();
            hasTransport = true;
        }

        return hasTransport;
    }
}
