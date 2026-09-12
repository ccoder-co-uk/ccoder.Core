// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Brokers.HomeSessions;

internal interface IHomeSessionBroker
{
    HomeSessionContext CreateHomeSessionContext(HttpContext context);
    bool IsSessionAvailable(HttpContext context);
    bool ContainsSessionKey(HttpContext context, string key);
    string SelectSessionValue(HttpContext context, string key);
    void SetSessionValue(HttpContext context, string key, string value);
    void RemoveSessionValue(HttpContext context, string key);
    void AbortRequest(HttpContext context);
    bool IsLocalUrl(IUrlHelper urlHelper, string url);
}