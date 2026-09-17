// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Foundations.Setup;

internal sealed partial class SetupRequestHostService
{
    private static void ValidateHostOnNormalize(string host)
    {
        if (string.IsNullOrWhiteSpace(value: host))
        {
            throw new ArgumentException(
                message: "A setup request host is required.",
                paramName: nameof(host));
        }
    }
}