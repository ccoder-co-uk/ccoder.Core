// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Brokers.HomeSessions;
using Web.Models;

namespace Web.Services.Foundations.HomeSessions;

internal sealed partial class HomeSessionService(
    IHomeSessionBroker homeSessionBroker)
        : IHomeSessionService
{
    public HomeSessionContext CreateHomeSessionContext(HttpContext context) =>
        TryCatch(operation: () =>
        {
            ValidateContext(context: context);

            return homeSessionBroker.CreateHomeSessionContext(
                context: context);
        });

    public bool CanUseSession(HttpContext context) =>
        TryCatch(operation: () =>
        {
            ValidateContext(context: context);

            return homeSessionBroker.IsSessionAvailable(context: context);
        });

    public bool ContainsSessionKey(HttpContext context, string key) =>
        TryCatch(operation: () =>
        {
            ValidateSession(context: context, key: key);

            return homeSessionBroker.ContainsSessionKey(
                context: context,
                key: key);
        });

    public string ReadSessionValue(HttpContext context, string key) =>
        TryCatch(operation: () =>
        {
            ValidateSession(context: context, key: key);

            return homeSessionBroker.SelectSessionValue(
                context: context,
                key: key);
        });

    public void SetSessionValue(
        HttpContext context,
        string key,
        string value) =>
        TryCatch(operation: () =>
        {
            ValidateSessionValue(
                context: context,
                key: key,
                value: value);

            homeSessionBroker.SetSessionValue(
                context: context,
                key: key,
                value: value);
        });

    public void RemoveSessionValue(HttpContext context, string key) =>
        TryCatch(operation: () =>
        {
            ValidateSession(context: context, key: key);

            homeSessionBroker.RemoveSessionValue(
                context: context,
                key: key);
        });

    public void AbortRequest(HttpContext context) =>
        TryCatch(operation: () =>
        {
            ValidateContext(context: context);

            homeSessionBroker.AbortRequest(context: context);
        });

    public bool IsLocalUrl(IUrlHelper urlHelper, string url) =>
        TryCatch(operation: () =>
        {
            ValidateUrl(urlHelper: urlHelper, url: url);

            return homeSessionBroker.IsLocalUrl(
                urlHelper: urlHelper,
                url: url);
        });
}