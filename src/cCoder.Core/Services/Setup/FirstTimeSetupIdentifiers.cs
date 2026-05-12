using System.Text.RegularExpressions;

namespace cCoder.Core.Services.Setup;

internal static partial class FirstTimeSetupIdentifiers
{
    public static string BuildTenantId(string tenantName)
    {
        string slug = UnsafeTenantSlug().Replace(tenantName?.Trim() ?? string.Empty, "-")
            .Trim('-')
            .ToLowerInvariant();

        return string.IsNullOrWhiteSpace(slug) ? "default" : slug;
    }

    public static string BuildUserId(string email) =>
        (email ?? string.Empty).Split('@', 2, StringSplitOptions.TrimEntries)[0].Trim();

    [GeneratedRegex("[^a-zA-Z0-9]+", RegexOptions.Compiled)]
    private static partial Regex UnsafeTenantSlug();
}
