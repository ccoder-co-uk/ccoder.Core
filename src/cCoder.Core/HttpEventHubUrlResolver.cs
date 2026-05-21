using Microsoft.Extensions.Configuration;

namespace cCoder.Core;

public static class HttpEventHubUrlResolver
{
    private const string DefaultHubPath = "Api/Eventing";

    public static string Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string explicitHubUrl = configuration.GetValue<string>("Eventing:Http:HubUrl");

        if (!string.IsNullOrWhiteSpace(explicitHubUrl))
            return Normalize(explicitHubUrl);

        if (!(configuration.GetValue<bool?>("Settings:enableExternalEventing") ?? true))
            return string.Empty;

        string hostedServicesRoot = configuration.GetValue<string>("Services:HostedServices");

        return string.IsNullOrWhiteSpace(hostedServicesRoot)
            ? null
            : Normalize(hostedServicesRoot);
    }

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            return value;

        string path = uri.AbsolutePath?.Trim('/') ?? string.Empty;

        if (string.Equals(path, DefaultHubPath, StringComparison.OrdinalIgnoreCase))
            return uri.ToString();

        if (string.IsNullOrWhiteSpace(path))
            return $"{value.TrimEnd('/')}/{DefaultHubPath}";

        return uri.ToString();
    }
}
