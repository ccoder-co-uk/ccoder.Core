// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Services.Processings;

internal sealed partial class HomeSessionProcessingService
    : IHomeSessionProcessingService
{
    public bool CanUseSession(
        HttpContext context) =>
        TryCatch(operation: () =>
        {
            ValidateContextOnCheck(
                context: context);

            return IsSessionAvailable(
                context: context);
        });

    public string GetSessionValue(
        HttpContext context,
        string key) =>
        TryCatch(operation: () =>
        {
            ValidateSessionOnGet(context: context, key: key);

            bool hasValue = IsSessionAvailable(context: context) && context.Session.Keys.Contains(value: key.ToLowerInvariant());

            return hasValue
                ? context.Session.GetString(key: key)
                : null;
        });

    public void SetSessionValue(
        HttpContext context,
        string key,
        string value) =>
        TryCatch(operation: () =>
        {
            ValidateSessionOnSet(context: context, key: key, value: value);

            if (!IsSessionAvailable(context: context))
            {
                return;
            }

            if (value is not null)
            {
                context.Session.SetString(
                    key: key.ToLowerInvariant(),
                    value: value);

                return;
            }

            if (context.Session.Keys.Contains(
                value: key.ToLowerInvariant()))
            {
                context.Session.Remove(
                    key: key.ToLowerInvariant());
            }
        });

    private static bool IsSessionAvailable(
        HttpContext context)
    {
        try
        {
            return context.Session?.IsAvailable == true;
        }
        catch
        {
            return false;
        }
    }
}