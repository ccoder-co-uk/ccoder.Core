// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Extensions.Configuration;

namespace cCoder.Core;

public static class HttpEventHubUrlResolver
{
    private const string DefaultHubPath = "Api/Eventing";

    public static string Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(argument: configuration);

        string explicitHubUrl = configuration.GetValue<string>(key: "Eventing:Http:HubUrl");

        if (!string.IsNullOrWhiteSpace(value: explicitHubUrl))
        {
            return Normalize(value: explicitHubUrl);
        }

        if (!(configuration.GetValue<bool?>(key: "Settings:enableExternalEventing") ?? true))
        {
            return string.Empty;
        }

        string hostedServicesRoot = configuration.GetValue<string>(key: "Services:HostedServices");

        return string.IsNullOrWhiteSpace(value: hostedServicesRoot)
            ? null
            : Normalize(value: hostedServicesRoot);
    }

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value: value))
        {
            return value;
        }

        if (!Uri.TryCreate(uriString: value, uriKind: UriKind.Absolute, result: out Uri uri))
        {
            return value;
        }

        string path = uri.AbsolutePath?.Trim(trimChar: '/') ?? string.Empty;

        if (string.Equals(a: path, b: DefaultHubPath, comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return uri.ToString();
        }

        if (string.IsNullOrWhiteSpace(value: path))
        {
            return $"{value.TrimEnd(trimChar: '/')}/{DefaultHubPath}";
        }

        return uri.ToString();
    }
}