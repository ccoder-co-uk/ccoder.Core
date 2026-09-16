// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text;
using System.Text.Json;
using cCoder.ContentManagement.Exposures;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Dependencies.AllowedOrigins;

internal sealed class AllowedOriginStoreDependency(
    IHttpContextAccessor httpContextAccessor,
    IAppManager appManager)
    : IHttpContextAccessor
{
    private static readonly Encoding JsonEncoding = Encoding.UTF8;

    private static readonly string[] OriginPropertyNames =
    [
        "allowedorigin", "allowedorigins", "origin", "origins", "domain",
        "domains", "host", "hosts", "url", "urls"
    ];

    HttpContext IHttpContextAccessor.HttpContext
    {
        get => httpContextAccessor.HttpContext;
        set => httpContextAccessor.HttpContext = value;
    }

    internal IEnumerable<string> GetAllowedOrigins()
    {
        string domain = httpContextAccessor.HttpContext?.Request?.Host.Host;

        if (string.IsNullOrWhiteSpace(value: domain))
        {
            return [];
        }

        App app = appManager.GetByDomain(
            domain: domain,
            ignoreFilters: true);

        if (app is null)
        {
            return [];
        }

        List<string> origins = [];

        if (!string.IsNullOrWhiteSpace(value: app.Domain))
        {
            origins.Add(item: app.Domain);
        }

        origins.AddRange(collection: ExtractOrigins(configJson: app.ConfigJson));

        return origins;
    }

    private static IEnumerable<string> ExtractOrigins(string configJson)
    {
        if (string.IsNullOrWhiteSpace(value: configJson))
        {
            return [];
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                utf8Json: JsonEncoding.GetBytes(s: configJson));
            return [.. ExtractOrigins(
                element: document.RootElement,
                propertyName: null)];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IEnumerable<string> ExtractOrigins(
        JsonElement element,
        string propertyName)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    foreach (string origin in ExtractOrigins(
                        element: property.Value,
                        propertyName: property.Name))
                    {
                        yield return origin;
                    }
                }

                break;

            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    foreach (string origin in ExtractOrigins(
                        element: item,
                        propertyName: propertyName))
                    {
                        yield return origin;
                    }
                }

                break;

            case JsonValueKind.String:
                string value = element.GetString();

                if (ShouldIncludeString(
                    propertyName: propertyName,
                    value: value))
                {
                    yield return value;
                }

                break;
        }
    }

    private static bool ShouldIncludeString(
        string propertyName,
        string value) =>
        IsOriginProperty(propertyName: propertyName)
        || LooksLikeOrigin(value: value);

    private static bool IsOriginProperty(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value: propertyName))
        {
            return false;
        }

        string normalized = new(
            [.. propertyName.Where(predicate: char.IsLetterOrDigit)
                .Select(selector: char.ToLowerInvariant)]);

        return OriginPropertyNames.Any(predicate: name => normalized.Contains(
            value: name));
    }

    private static bool LooksLikeOrigin(string value)
    {
        if (string.IsNullOrWhiteSpace(value: value))
        {
            return false;
        }

        string candidate = value.Trim().TrimEnd(trimChar: '/');

        if (candidate.Contains(value: "://", comparisonType: StringComparison.Ordinal)
            && Uri.TryCreate(
                uriString: candidate,
                uriKind: UriKind.Absolute,
                result: out Uri uri))
        {
            return uri.Scheme is "http" or "https";
        }

        return candidate.Contains(value: '.', comparisonType: StringComparison.Ordinal)
            || candidate.Contains(value: ':', comparisonType: StringComparison.Ordinal);
    }
}