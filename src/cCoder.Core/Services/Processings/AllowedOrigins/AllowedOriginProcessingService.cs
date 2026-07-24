// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Processings.AllowedOrigins;

internal sealed class AllowedOriginProcessingService : IAllowedOriginProcessingService
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public CoreAllowedOriginSnapshot CreateSnapshot(IEnumerable<string> configuredOrigins)
    {
        HashSet<string> exactOrigins = new(Comparer);
        HashSet<string> authorities = new(Comparer);
        HashSet<string> hosts = new(Comparer);

        foreach (string configuredOrigin in configuredOrigins ?? [])
        {
            string candidate = configuredOrigin?.Trim();

            if (string.IsNullOrWhiteSpace(value: candidate))
            {
                continue;
            }

            candidate = candidate.TrimEnd(trimChar: '/');

            if (candidate.Contains(value: "://", comparisonType: StringComparison.Ordinal)
                && Uri.TryCreate(uriString: candidate, uriKind: UriKind.Absolute, result: out Uri absoluteUri))
            {
                if (IsSupportedScheme(uri: absoluteUri))
                {
                    exactOrigins.Add(item: NormalizeOrigin(uri: absoluteUri));
                }

                continue;
            }

            if (Uri.CheckHostName(name: candidate.Split(separator: ':')[0]) == UriHostNameType.Unknown)
            {
                continue;
            }

            if (candidate.Contains(value: ':', comparisonType: StringComparison.Ordinal))
            {
                authorities.Add(item: candidate.ToLowerInvariant());
            }
            else
            {
                hosts.Add(item: candidate.ToLowerInvariant());
            }
        }

        return new CoreAllowedOriginSnapshot(exactOrigins, authorities, hosts);
    }

    public bool IsAllowed(string origin, CoreAllowedOriginSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(value: origin))
        {
            return false;
        }

        if (!Uri.TryCreate(uriString: origin, uriKind: UriKind.Absolute, result: out Uri parsedOrigin))
        {
            return false;
        }

        if (!IsSupportedScheme(uri: parsedOrigin))
        {
            return false;
        }

        if (IsLoopback(uri: parsedOrigin))
        {
            return true;
        }

        return snapshot.ExactOrigins.Contains(item: NormalizeOrigin(uri: parsedOrigin))
            || snapshot.Authorities.Contains(item: parsedOrigin.Authority.ToLowerInvariant())
            || snapshot.Hosts.Contains(item: parsedOrigin.Host.ToLowerInvariant());
    }

    private static bool IsSupportedScheme(Uri uri) =>
        uri.Scheme is "http" or "https";

    private static string NormalizeOrigin(Uri uri) =>
        $"{uri.Scheme.ToLowerInvariant()}://{uri.Authority.ToLowerInvariant()}";

    private static bool IsLoopback(Uri uri) =>
        string.Equals(a: uri.Host, b: "localhost", comparisonType: StringComparison.OrdinalIgnoreCase)
        || (IPAddress.TryParse(ipString: uri.Host, address: out IPAddress address) && IPAddress.IsLoopback(address: address));
}