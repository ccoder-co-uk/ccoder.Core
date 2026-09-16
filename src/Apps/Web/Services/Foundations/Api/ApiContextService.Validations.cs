// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.IO;

namespace Web.Services.Foundations.Api;

internal sealed partial class ApiContextService
{
    private static void ValidateApiInfosOnGet()
    {
    }

    private static void ValidateRequestBodyOnRead(Stream requestBody) =>
        ArgumentNullException.ThrowIfNull(argument: requestBody);
}