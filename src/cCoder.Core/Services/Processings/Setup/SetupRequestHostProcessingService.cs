// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Processings.Setup;

internal sealed partial class SetupRequestHostProcessingService
    : ISetupRequestHostProcessingService
{
    public string NormalizeHost(string host) =>
        TryCatch(operation: () =>
        {
            ValidateHost(host: host);

            return host
                .Trim()
                .Replace(
                    oldValue: "www.",
                    newValue: string.Empty,
                    comparisonType: StringComparison.OrdinalIgnoreCase)
                .ToLowerInvariant();
        });
}