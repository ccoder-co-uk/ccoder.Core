// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.Http;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AllowedOrigins;

internal sealed partial class AllowedOriginStoreService(
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

    public ValueTask<string[]> GetAllowedOriginsAsync() =>
        TryCatch(operation: () =>
        {
            ValidateAllowedOriginsOnGet();

            HttpRequest request = httpRequestBroker.GetCurrentRequest();
            string domain = request?.Host.Host;

            if (!string.IsNullOrWhiteSpace(value: domain))
            {
                App app = appBroker.GetAppByDomain(
                    domain: domain,
                    ignoreFilters: true);

                string[] origins = app is null
                    ? []
                    : [.. GetAllowedOrigins(app: app)
                        .Where(predicate: origin => !string.IsNullOrWhiteSpace(value: origin))
                        .Distinct(comparer: StringComparer.OrdinalIgnoreCase)];

                return ValueTask.FromResult(result: origins);
            }

            return ValueTask.FromResult(result: Array.Empty<string>());
        });

    private static IEnumerable<string> GetAllowedOrigins(App app)
    {
        if (!string.IsNullOrWhiteSpace(value: app.Domain))
        {
            yield return app.Domain;
        }

        foreach (string origin in ExtractOriginsFromConfigJson(configJson: app.ConfigJson))
        {
            yield return origin;
        }
    }

    internal static IEnumerable<string> ExtractOriginsFromConfigJson(string configJson)
    {
        if (string.IsNullOrWhiteSpace(value: configJson))
        {
            return [];
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json: configJson);

            return [.. ExtractOrigins(element: document.RootElement, propertyName: null)];
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
                    foreach (string origin in ExtractOrigins(element: property.Value, propertyName: property.Name))
                    {
                        yield return origin;
                    }
                }

                break;

            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    foreach (string origin in ExtractOrigins(element: item, propertyName: propertyName))
                    {
                        yield return origin;
                    }
                }

                break;

            case JsonValueKind.String:
                string value = element.GetString();

                if (ShouldIncludeString(propertyName: propertyName, value: value))
                {
                    yield return value;
                }

                break;
        }
    }

    private static bool ShouldIncludeString(string propertyName, string value) =>
        IsOriginProperty(propertyName: propertyName) || LooksLikeOrigin(value: value);

    private static bool IsOriginProperty(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value: propertyName))
        {
            return false;
        }

        string normalized = new(
            [.. propertyName
                .Where(predicate: char.IsLetterOrDigit)
                .Select(selector: char.ToLowerInvariant)]);

        return OriginPropertyNames.Any(predicate: name => normalized.Contains(value: name));
    }

    private static bool LooksLikeOrigin(string value)
    {
        if (string.IsNullOrWhiteSpace(value: value))
        {
            return false;
        }

        string candidate = value.Trim()
            .TrimEnd(trimChar: '/');

        if (candidate.Contains(value: "://", comparisonType: StringComparison.Ordinal)
            && Uri.TryCreate(uriString: candidate, uriKind: UriKind.Absolute, result: out Uri uri))
        {
            return uri.Scheme is "http" or "https";
        }

        return candidate.Contains(value: '.', comparisonType: StringComparison.Ordinal)
            || candidate.Contains(value: ':', comparisonType: StringComparison.Ordinal);
    }
}