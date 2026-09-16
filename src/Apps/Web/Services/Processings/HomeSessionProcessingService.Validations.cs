// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Services.Processings;

internal sealed partial class HomeSessionProcessingService
{
    private static void Validate(params object[] inputs)
    {
        if (inputs.Any(predicate: input => input is null))
        {
            throw new ArgumentNullException(paramName: nameof(inputs));
        }
    }

    private static void ValidateContextOnCheck(
        HttpContext context) =>
        Validate(inputs: [context]);

    private static void ValidateContextOnCreate(
        HttpContext context) =>
        Validate(inputs: [context]);

    private static void ValidateSessionOnGet(
        HttpContext context,
        string key)
    {
        Validate(inputs: [context, key]);

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
        Validate(inputs: [context, key]);

    private static void ValidateSessionValueOnGet(
        HttpContext context,
        string key)
    {
        Validate(inputs: [context, key]);

        if (string.IsNullOrWhiteSpace(value: key))
        {
            throw new ArgumentException(
                message: "A session key is required.",
                paramName: nameof(key));
        }
    }

    private static void ValidateUrlOnCheck(
        Microsoft.AspNetCore.Mvc.IUrlHelper urlHelper,
        string url)
    {
        Validate(inputs: [urlHelper, url]);
    }
}