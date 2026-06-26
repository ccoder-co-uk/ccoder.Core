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

            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            candidate = candidate.TrimEnd('/');

            if (candidate.Contains("://", StringComparison.Ordinal)
                && Uri.TryCreate(candidate, UriKind.Absolute, out Uri absoluteUri))
            {
                if (IsSupportedScheme(absoluteUri))
                    exactOrigins.Add(NormalizeOrigin(absoluteUri));

                continue;
            }

            if (Uri.CheckHostName(candidate.Split(':')[0]) == UriHostNameType.Unknown)
                continue;

            if (candidate.Contains(':', StringComparison.Ordinal))
                authorities.Add(candidate.ToLowerInvariant());
            else
                hosts.Add(candidate.ToLowerInvariant());
        }

        return new CoreAllowedOriginSnapshot(exactOrigins, authorities, hosts);
    }

    public bool IsAllowed(string origin, CoreAllowedOriginSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return false;

        if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri parsedOrigin))
            return false;

        if (!IsSupportedScheme(parsedOrigin))
            return false;

        if (IsLoopback(parsedOrigin))
            return true;

        return snapshot.ExactOrigins.Contains(NormalizeOrigin(parsedOrigin))
            || snapshot.Authorities.Contains(parsedOrigin.Authority.ToLowerInvariant())
            || snapshot.Hosts.Contains(parsedOrigin.Host.ToLowerInvariant());
    }

    private static bool IsSupportedScheme(Uri uri) =>
        uri.Scheme is "http" or "https";

    private static string NormalizeOrigin(Uri uri) =>
        $"{uri.Scheme.ToLowerInvariant()}://{uri.Authority.ToLowerInvariant()}";

    private static bool IsLoopback(Uri uri) =>
        string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
        || (IPAddress.TryParse(uri.Host, out IPAddress address) && IPAddress.IsLoopback(address));
}
