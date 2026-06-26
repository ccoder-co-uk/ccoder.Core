using cCoder.Core;
using cCoder.Eventing.Http.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using cCoder.Workflow.Services.Orchestrations;

namespace HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IConfiguration config = ConfigureApplication(builder.Configuration, builder.Environment);

        builder.Services.AddCoreHostedServices(coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(coreConfig =>
            {
                coreConfig.CoreConnectionString = config.GetValue<string>("ConnectionStrings:Core");
                coreConfig.SecurityConnectionString = config.GetValue<string>("ConnectionStrings:SSO");
                coreConfig.DecryptionKey = config.GetValue<string>("Settings:DecryptionKey");
                coreConfig.CacheSource = config.GetValue<string>("Settings:CacheSource");
                coreConfig.CacheSourceAppId = config.GetValue<int?>("Settings:CacheSourceAppId");
                coreConfig.CacheExpiry = config.GetValue<int?>("Settings:CacheExpiry");
                coreConfig.SslPort = config.GetValue<int?>("Settings:sslPort");
                coreConfig.WorkflowServiceUrl = config.GetValue<string>("Services:Workflow");
                coreConfig.EventProviderType = ResolveEventProviderType(config);
                coreConfig.HttpEventHubUrl = HttpEventHubUrlResolver.Resolve(config);
                coreConfig.ServiceBusConnectionString = config.GetConnectionString("ServiceBus");
                coreConfig.EnableHttpEventing = IsHttpEventProvider(coreConfig.EventProviderType);
                coreConfig.EnableServiceBusEventing = IsServiceBusEventProvider(coreConfig.EventProviderType)
                    && !string.IsNullOrWhiteSpace(coreConfig.ServiceBusConnectionString);
                coreConfig.MaxConcurrency = ResolveMaxConcurrency(config, coreConfig.EventProviderType);
                coreConfig.DebugInfo = config.GetValue<bool>("DebugInfo");
                coreConfig.LogSQL = config.GetValue<bool>("LogSQL");
            });
        });
        builder.Services.AddControllers()
            .ConfigureApplicationPartManager(manager =>
                manager.FeatureProviders.Add(
                    new ExcludeHttpEventControllerFeatureProvider(typeof(HttpEventController))));
        builder.Services.AddScoped<ReceivedHttpEventProcessor>();
        builder.Services.RemoveAll<IWorkflowInstanceManagementOrchestrationService>();
        builder.Services.AddTransient<IWorkflowInstanceManagementOrchestrationService, HostedServicesWorkflowInstanceManagementOrchestrationService>();

        WebApplication app = builder.Build();
        app.StartCoreHostedServices();
        app.Run();
    }

    private static IConfiguration ConfigureApplication(ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return configuration;
    }

    private static string ResolveEventProviderType(IConfiguration config) =>
        config.GetValue<string>("Eventing:ProviderType")
        ?? config.GetValue<string>("Eventing:Provider")
        ?? "Http";

    private static int ResolveMaxConcurrency(IConfiguration config, string eventProviderType) =>
        IsServiceBusEventProvider(eventProviderType)
            ? config.GetValue<int?>("Eventing:ServiceBus:MaxConcurrency") ?? 1
            : config.GetValue<int?>("Eventing:Http:MaxConcurrency") ?? 1;

    private static bool IsServiceBusEventProvider(string eventProviderType) =>
        string.Equals(eventProviderType, "ServiceBus", StringComparison.OrdinalIgnoreCase);

    private static bool IsHttpEventProvider(string eventProviderType) =>
        string.Equals(eventProviderType, "Http", StringComparison.OrdinalIgnoreCase);

    private sealed class ExcludeHttpEventControllerFeatureProvider(Type controllerType)
        : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(
            IEnumerable<ApplicationPart> parts,
            ControllerFeature feature)
        {
            for (int index = feature.Controllers.Count - 1; index >= 0; index--)
            {
                if (feature.Controllers[index].AsType() == controllerType)
                    feature.Controllers.RemoveAt(index);
            }
        }
    }
}
