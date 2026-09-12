// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Dependencies;
using Web.Models;

namespace Web.Brokers.HomeSessions;

internal sealed class HomeSessionBroker(
    HomeSessionDependency homeSessionDependency)
        : IHomeSessionBroker
{
    public HomeSessionContext CreateHomeSessionContext(HttpContext context) =>
        homeSessionDependency.CreateHomeSessionContext(context: context);

    public bool IsSessionAvailable(HttpContext context) =>
        homeSessionDependency.IsSessionAvailable(context: context);

    public bool ContainsSessionKey(HttpContext context, string key) =>
        homeSessionDependency.ContainsSessionKey(context: context, key: key);

    public string SelectSessionValue(HttpContext context, string key) =>
        homeSessionDependency.SelectSessionValue(context: context, key: key);

    public void SetSessionValue(
        HttpContext context,
        string key,
        string value) =>
        homeSessionDependency.SetSessionValue(
            context: context,
            key: key,
            value: value);

    public void RemoveSessionValue(HttpContext context, string key) =>
        homeSessionDependency.RemoveSessionValue(context: context, key: key);

    public void AbortRequest(HttpContext context) =>
        homeSessionDependency.AbortRequest(context: context);

    public bool IsLocalUrl(IUrlHelper urlHelper, string url) =>
        homeSessionDependency.IsLocalUrl(urlHelper: urlHelper, url: url);
}