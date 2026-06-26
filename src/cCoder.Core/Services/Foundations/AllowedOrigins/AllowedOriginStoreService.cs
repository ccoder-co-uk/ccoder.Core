using System.Text.Json;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.Http;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AllowedOrigins;

internal sealed class AllowedOriginStoreService(
    IContentManagementAppBroker appBroker,
    IHttpRequestBroker httpRequestBroker)
    : IAllowedOriginStoreService
{
    private static readonly string[] OriginPropertyNames =
    [
        "allowedorigin",
        "allowedorigins",
        "origin",
        "origins",
        "domain",
        "domains",
        "host",
        "hosts",
        "url",
        "urls"
    ];

    public async ValueTask<string[]> GetAllowedOriginsAsync()
    {
        HttpRequest request = httpRequestBroker.GetCurrentRequest();
        string domain = request?.Host.Host;

        if (!string.IsNullOrWhiteSpace(domain))
        {
            App app = appBroker.GetByDomain(domain, ignoreFilters: true);

            string[] origins = app is null
                ? []
                : GetAllowedOrigins(app)
                    .Where(origin => !string.IsNullOrWhiteSpace(origin))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

            return await ValueTask.FromResult(origins);
        }

        return await ValueTask.FromResult(Array.Empty<string>());
    }

    private static IEnumerable<string> GetAllowedOrigins(App app)
    {
        if (!string.IsNullOrWhiteSpace(app.Domain))
            yield return app.Domain;

        foreach (string origin in ExtractOriginsFromConfigJson(app.ConfigJson))
            yield return origin;
    }

    internal static IEnumerable<string> ExtractOriginsFromConfigJson(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return [];

        try
        {
            using JsonDocument document = JsonDocument.Parse(configJson);

            return ExtractOrigins(document.RootElement, propertyName: null)
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IEnumerable<string> ExtractOrigins(JsonElement element, string propertyName)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    foreach (string origin in ExtractOrigins(property.Value, property.Name))
                        yield return origin;
                }

                break;

            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    foreach (string origin in ExtractOrigins(item, propertyName))
                        yield return origin;
                }

                break;

            case JsonValueKind.String:
                string value = element.GetString();

                if (ShouldIncludeString(propertyName, value))
                    yield return value;

                break;
        }
    }

    private static bool ShouldIncludeString(string propertyName, string value) =>
        IsOriginProperty(propertyName) || LooksLikeOrigin(value);

    private static bool IsOriginProperty(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return false;

        string normalized = new(
            propertyName
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());

        return OriginPropertyNames.Any(name => normalized.Contains(name));
    }

    private static bool LooksLikeOrigin(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string candidate = value.Trim().TrimEnd('/');

        if (candidate.Contains("://", StringComparison.Ordinal)
            && Uri.TryCreate(candidate, UriKind.Absolute, out Uri uri))
        {
            return uri.Scheme is "http" or "https";
        }

        return candidate.Contains('.', StringComparison.Ordinal)
            || candidate.Contains(':', StringComparison.Ordinal);
    }
}
