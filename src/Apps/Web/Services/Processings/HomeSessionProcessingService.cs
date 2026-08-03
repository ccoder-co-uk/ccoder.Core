// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Dynamic;
using cCoder.Core.Models;
using cCoder.Data;

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

    public ExpandoObject CreateExpandoObject(
        HttpContext context) =>
        TryCatch(operation: () =>
        {
            ValidateContextOnCreate(
                context: context);

            dynamic result = new ExpandoObject();

            IDictionary<string, object> values =
                (IDictionary<string, object>)result;

            string host = context.Request.Host.Host
                .Replace(
                    oldValue: "www.",
                    newValue: string.Empty)
                .ToLowerInvariant();

            int? port = context.Request.Host.Port;

            result.apiRoot =
                port.HasValue && port.Value is not 443 and not 80
                    ? $"{context.Request.Scheme}://{host}:{port.Value}/Api/"
                    : $"{context.Request.Scheme}://{host}/Api/";

            ICoreAuthInfo authInfo = context.RequestServices
                .GetService<ICoreAuthInfo>()
                ?? new CoreAuthInfo
                {
                    SSOUserId = "Guest"
                };

            if (!string.IsNullOrWhiteSpace(
                value: authInfo.SSOUserId)
                && !string.Equals(
                    a: authInfo.SSOUserId,
                    b: "Guest",
                    comparisonType:
                        StringComparison.OrdinalIgnoreCase))
            {
                values["user"] = authInfo.SSOUserId;
            }

            string token =
                context.Request.Query["t"].ToString();

            if (!string.IsNullOrWhiteSpace(value: token))
            {
                values["token"] = token;
            }

            if (!IsSessionAvailable(context: context))
            {
                return result;
            }

            foreach (string key in context.Session.Keys)
            {
                values[key] = key == "ssoUser"
                    ? authInfo.SSOUserId
                    : GetSessionValueCore(
                        context: context,
                        key: key);
            }

            return (ExpandoObject)result;
        });

    public string GetSessionValue(
        HttpContext context,
        string key) =>
        TryCatch(operation: () =>
        {
            ValidateSessionOnGet(context: context, key: key);

            return GetSessionValueCore(
                context: context,
                key: key);
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

    private static string GetSessionValueCore(
        HttpContext context,
        string key)
    {
        bool hasValue = IsSessionAvailable(context: context)
            && context.Session.Keys.Contains(
                value: key.ToLowerInvariant());

        return hasValue
            ? context.Session.GetString(key: key)
            : null;
    }
}