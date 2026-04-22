namespace Web.Services.Setup;

internal static class SetupRequestHostNormalizer
{
    public static string Normalize(string host) =>
        (host ?? string.Empty)
            .Trim()
            .Replace("www.", string.Empty, StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();
}
