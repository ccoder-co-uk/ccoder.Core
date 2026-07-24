// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Setup;

public static class SetupRequestHostNormalizer
{
    public static string Normalize(string host) =>
        (host ?? string.Empty)
            .Trim()
            .Replace(oldValue: "www.", newValue: string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();
}