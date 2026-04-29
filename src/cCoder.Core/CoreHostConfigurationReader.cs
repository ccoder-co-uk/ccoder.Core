using cCoder.Core.Models;

namespace cCoder.Core;

public static class CoreHostConfigurationReader
{
    public static CoreHostConfiguration ReadForWeb(IConfiguration configuration)
    {
        string explicitHubUrl = configuration.GetValue<string>("Eventing:Http:HubUrl") ?? string.Empty;
        bool enableExternalEventing = configuration.GetValue<bool?>("Settings:enableExternalEventing") ?? true;
        string hostedServicesRoot = configuration.GetValue<string>("Services:HostedServices") ?? string.Empty;
        string resolvedHubUrl = !string.IsNullOrWhiteSpace(explicitHubUrl)
            ? explicitHubUrl
            : !enableExternalEventing
                ? string.Empty
                : string.IsNullOrWhiteSpace(hostedServicesRoot)
                    ? string.Empty
                    : $"{hostedServicesRoot.TrimEnd('/')}/Api/Eventing";

        return Create(configuration, resolvedHubUrl);
    }

    public static CoreHostConfiguration ReadForHostedServices(IConfiguration configuration) =>
        Create(configuration, string.Empty);

    private static CoreHostConfiguration Create(
        IConfiguration configuration,
        string httpEventHubUrl) =>
        new()
        {
            CoreConnectionString = configuration.GetValue<string>("ConnectionStrings:Core") ?? string.Empty,
            SecurityConnectionString = configuration.GetValue<string>("ConnectionStrings:SSO") ?? string.Empty,
            DecryptionKey = configuration.GetValue<string>("Settings:DecryptionKey") ?? string.Empty,
            CacheSource = configuration.GetValue<string>("Settings:CacheSource") ?? string.Empty,
            CacheSourceAppId = configuration.GetValue<int?>("Settings:CacheSourceAppId"),
            CacheExpiry = configuration.GetValue<int?>("Settings:CacheExpiry"),
            SslPort = configuration.GetValue<int?>("Settings:sslPort"),
            WorkflowServiceUrl = configuration.GetValue<string>("Services:Workflow") ?? string.Empty,
            HttpEventHubUrl = httpEventHubUrl,
            EnableHttpEventing = !string.IsNullOrWhiteSpace(httpEventHubUrl),
            MaxConcurrency = configuration.GetValue<int?>("Eventing:Http:MaxConcurrency") ?? 1,
            DebugInfo = configuration.GetValue<bool>("DebugInfo"),
            LogSQL = configuration.GetValue<bool>("LogSQL"),
        };
}
