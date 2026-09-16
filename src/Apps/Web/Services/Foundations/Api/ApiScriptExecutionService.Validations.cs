// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.IO;

namespace Web.Services.Foundations.Api;

internal sealed partial class ApiScriptExecutionService
{
    private static void ValidateScriptOnExecute(string script)
    {
        if (string.IsNullOrWhiteSpace(value: script))
        {
            throw new ArgumentException(
                message: "A script is required.",
                paramName: nameof(script));
        }
    }

    private static void ValidateRequestBodyOnRead(Stream requestBody) =>
        ArgumentNullException.ThrowIfNull(argument: requestBody);
}