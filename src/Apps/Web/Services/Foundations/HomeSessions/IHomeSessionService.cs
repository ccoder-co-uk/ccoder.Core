// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Services.Foundations.HomeSessions;

internal interface IHomeSessionService
{
    HomeSessionContext CreateHomeSessionContext(HttpContext context);
    bool CanUseSession(HttpContext context);
    bool ContainsSessionKey(HttpContext context, string key);
    string ReadSessionValue(HttpContext context, string key);
    void SetSessionValue(HttpContext context, string key, string value);
    void RemoveSessionValue(HttpContext context, string key);
    void AbortRequest(HttpContext context);
    bool IsLocalUrl(IUrlHelper urlHelper, string url);
}