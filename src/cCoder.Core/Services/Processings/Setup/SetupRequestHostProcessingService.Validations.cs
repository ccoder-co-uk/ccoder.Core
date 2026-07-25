// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Processings.Setup;

internal sealed partial class SetupRequestHostProcessingService
{
    private static void ValidateHost(string host)
    {
        if (string.IsNullOrWhiteSpace(value: host))
        {
            throw new ArgumentException(
                message: "A setup request host is required.",
                paramName: nameof(host));
        }
    }
}