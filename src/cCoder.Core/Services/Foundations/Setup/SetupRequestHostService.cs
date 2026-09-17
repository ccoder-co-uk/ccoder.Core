// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Foundations.Setup;

internal sealed partial class SetupRequestHostService
    : ISetupRequestHostService
{
    public string NormalizeHost(string host) =>
        TryCatch(operation: () =>
        {
            ValidateHostOnNormalize(host: host);

            return host
                .Trim()
                .Replace(
                    oldValue: "www.",
                    newValue: string.Empty,
                    comparisonType: StringComparison.OrdinalIgnoreCase)
                .ToLowerInvariant();
        });
}