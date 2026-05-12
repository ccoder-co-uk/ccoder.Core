namespace cCoder.Core.Services.Setup;

public static class SetupRequestHostNormalizer
{
    public static string Normalize(string host) =>
        (host ?? string.Empty)
            .Trim()
            .Replace("www.", string.Empty, StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();
}
