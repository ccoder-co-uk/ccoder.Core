// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Services.Processings;

internal sealed partial class HomeSessionProcessingService
{
    private static void ValidateContextOnCheck(
        HttpContext context) =>
        ArgumentNullException.ThrowIfNull(
            argument: context);

    private static void ValidateContextOnCreate(
        HttpContext context) =>
        ArgumentNullException.ThrowIfNull(
            argument: context);

    private static void ValidateSessionOnGet(
        HttpContext context,
        string key)
    {
        ArgumentNullException.ThrowIfNull(
            argument: context);

        if (string.IsNullOrWhiteSpace(value: key))
        {
            throw new ArgumentException(
                message: "A session key is required.",
                paramName: nameof(key));
        }
    }

    private static void ValidateSessionOnSet(
        HttpContext context,
        string key,
        string value) =>
        ValidateSessionOnGet(
            context: context,
            key: key);
}