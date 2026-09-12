// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services.Foundations.HomeSessions;

namespace Web.Services.Processings;

internal sealed partial class HomeSessionProcessingService(
    IHomeSessionService homeSessionService)
        : IHomeSessionProcessingService
{
    public bool CanUseSession(HttpContext context) =>
        TryCatch(operation: () =>
        {
            ValidateContextOnCheck(context: context);

            return homeSessionService.CanUseSession(context: context);
        });

    public ExpandoObject CreateExpandoObject(HttpContext context) =>
        TryCatch(operation: () =>
        {
            ValidateContextOnCreate(context: context);

            dynamic result = new ExpandoObject();

            IDictionary<string, object> values =
                (IDictionary<string, object>)result;

            HomeSessionContext homeSessionContext =
                homeSessionService.CreateHomeSessionContext(
                    context: context);

            string host = homeSessionContext.Host
                .Replace(
                    oldValue: "www.",
                    newValue: string.Empty)
                .ToLowerInvariant();

            int? port = homeSessionContext.Port;

            result.apiRoot =
                port.HasValue && port.Value is not 443 and not 80
                    ? $"{homeSessionContext.Scheme}://{host}:{port.Value}/Api/"
                    : $"{homeSessionContext.Scheme}://{host}/Api/";

            if (!string.IsNullOrWhiteSpace(
                value: homeSessionContext.SSOUserId)
                && !string.Equals(
                    a: homeSessionContext.SSOUserId,
                    b: "Guest",
                    comparisonType:
                        StringComparison.OrdinalIgnoreCase))
            {
                values["user"] = homeSessionContext.SSOUserId;
            }

            string token = homeSessionContext.Token;

            if (!string.IsNullOrWhiteSpace(value: token))
            {
                values["token"] = token;
            }

            if (!homeSessionService.CanUseSession(context: context))
            {
                return result;
            }

            foreach (string key in homeSessionContext.SessionKeys)
            {
                values[key] = key == "ssoUser"
                    ? homeSessionContext.SSOUserId
                    : GetSessionValueCore(
                        context: context,
                        key: key);
            }

            return (ExpandoObject)result;
        });

    public string GetSessionValue(HttpContext context, string key) =>
        TryCatch(operation: () =>
        {
            ValidateSessionOnGet(context: context, key: key);

            return GetSessionValueCore(context: context, key: key);
        });

    public void SetSessionValue(
        HttpContext context,
        string key,
        string value) =>
        TryCatch(operation: () =>
        {
            ValidateSessionOnSet(context: context, key: key, value: value);

            if (!homeSessionService.CanUseSession(context: context))
            {
                return;
            }

            if (value is not null)
            {
                homeSessionService.SetSessionValue(
                    context: context,
                    key: key,
                    value: value);

                return;
            }

            if (homeSessionService.ContainsSessionKey(
                context: context,
                key: key))
            {
                homeSessionService.RemoveSessionValue(
                    context: context,
                    key: key);
            }
        });

    public void AbortRequest(HttpContext context) =>
        TryCatch(operation: () =>
        {
            ValidateContextOnCheck(context: context);

            homeSessionService.AbortRequest(context: context);
        });

    public bool IsLocalUrl(IUrlHelper urlHelper, string url) =>
        TryCatch(operation: () =>
        {
            ValidateUrlOnCheck(urlHelper: urlHelper, url: url);

            return homeSessionService.IsLocalUrl(
                urlHelper: urlHelper,
                url: url);
        });

    private string GetSessionValueCore(HttpContext context, string key)
    {
        bool hasValue = homeSessionService.CanUseSession(context: context)
            && homeSessionService.ContainsSessionKey(
                context: context,
                key: key);

        return hasValue
            ? homeSessionService.ReadSessionValue(
                context: context,
                key: key)
            : null;
    }
}