// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace Web.Services.Foundations.HomeSessions;

internal sealed partial class HomeSessionService
{
    private static void ValidateContext(HttpContext context) =>
        ArgumentNullException.ThrowIfNull(argument: context);

    private static void ValidateSession(HttpContext context, string key)
    {
        ValidateContext(context: context);

        if (string.IsNullOrWhiteSpace(value: key))
        {
            throw new ArgumentException(
                message: "A session key is required.",
                paramName: nameof(key));
        }
    }

    private static void ValidateSessionValue(
        HttpContext context,
        string key,
        string value)
    {
        ValidateSession(context: context, key: key);
        _ = value;
    }

    private static void ValidateUrl(IUrlHelper urlHelper, string url)
    {
        ArgumentNullException.ThrowIfNull(argument: urlHelper);

        if (url is null)
        {
            throw new ArgumentNullException(paramName: nameof(url));
        }
    }
}