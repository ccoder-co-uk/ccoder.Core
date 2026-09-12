// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Data;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Dependencies;

internal sealed class HomeSessionDependency
{
    public HomeSessionContext CreateHomeSessionContext(HttpContext context)
    {
        ICoreAuthInfo authInfo = context.RequestServices
            .GetService<ICoreAuthInfo>()
            ?? new CoreAuthInfo
            {
                SSOUserId = "Guest"
            };

        return new HomeSessionContext
        {
            Host = context.Request.Host.Host,
            Port = context.Request.Host.Port,
            Scheme = context.Request.Scheme,
            SSOUserId = authInfo.SSOUserId,
            Token = context.Request.Query["t"].ToString(),
            SessionKeys = IsSessionAvailable(context: context)
                ? [.. context.Session.Keys]
                : []
        };
    }

    public bool IsSessionAvailable(HttpContext context)
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

    public bool ContainsSessionKey(HttpContext context, string key) =>
        context.Session.Keys.Contains(value: key.ToLowerInvariant());

    public string SelectSessionValue(HttpContext context, string key) =>
        context.Session.GetString(key: key);

    public void SetSessionValue(
        HttpContext context,
        string key,
        string value) =>
        context.Session.SetString(
            key: key.ToLowerInvariant(),
            value: value);

    public void RemoveSessionValue(HttpContext context, string key) =>
        context.Session.Remove(key: key.ToLowerInvariant());

    public void AbortRequest(HttpContext context) =>
        context.Abort();

    public bool IsLocalUrl(IUrlHelper urlHelper, string url) =>
        urlHelper.IsLocalUrl(url: url);
}