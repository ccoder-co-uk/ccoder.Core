// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading;
namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class FirstTimeSetupStateOrchestrationService
{
    private static void ValidateCancellationTokenOnCheck(
        CancellationToken cancellationToken)
    {
    }

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