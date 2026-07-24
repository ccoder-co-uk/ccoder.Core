// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.RegularExpressions;

namespace cCoder.Core.Services.Setup;

internal static partial class FirstTimeSetupIdentifiers
{
    public static string BuildTenantId(string tenantName)
    {
        string slug = UnsafeTenantSlug()
            .Replace(input: tenantName?.Trim() ?? string.Empty, replacement: "-")
            .Trim(trimChar: '-')
            .ToLowerInvariant();

        return string.IsNullOrWhiteSpace(value: slug) ? "default" : slug;
    }

    public static string BuildUserId(string email) =>
        (email ?? string.Empty).Split(separator: '@', count: 2, options: StringSplitOptions.TrimEntries)[0].Trim();

    [GeneratedRegex("[^a-zA-Z0-9]+", RegexOptions.Compiled)]
    private static partial Regex UnsafeTenantSlug();
}