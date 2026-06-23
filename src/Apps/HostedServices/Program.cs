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
                coreConfig.MaxConcurrency = config.GetValue<int?>("Eventing:Http:MaxConcurrency") ?? 1;
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
