// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using cCoder.Data;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dependencies;

internal sealed class HomeSessionDependency(
    IHttpContextAccessor httpContextAccessor)
    : IHttpContextAccessor
{
    HttpContext IHttpContextAccessor.HttpContext
    {
        get => httpContextAccessor.HttpContext;
        set => httpContextAccessor.HttpContext = value;
    }

    public string SelectRequestHost(HttpContext context) =>
        context.Request.Host.Host;

    public int? SelectRequestPort(HttpContext context) =>
        context.Request.Host.Port;

    public string SelectRequestScheme(HttpContext context) =>
        context.Request.Scheme;

    public string SelectSsoUserId(HttpContext context)
    {
        HttpContext requestContext =
            httpContextAccessor.HttpContext ?? context;

        ICoreAuthInfo authInfo = requestContext.RequestServices
            .GetService<ICoreAuthInfo>();

        return authInfo?.SSOUserId ?? "Guest";
    }

    public string SelectRequestToken(HttpContext context) =>
        context.Request.Query["t"].ToString();

    public string[] SelectSessionKeys(HttpContext context) =>
        IsSessionAvailable(context: context)
            ? [.. context.Session.Keys]
            : [];

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
        string value)
    {
        string normalizedKey = key.ToLowerInvariant();

        if (string.Equals(
            a: context.Session.GetString(key: normalizedKey),
            b: value,
            comparisonType: StringComparison.Ordinal))
        {
            return;
        }

        context.Session.SetString(
            key: normalizedKey,
            value: value);
    }

    public void RemoveSessionValue(HttpContext context, string key) =>
        context.Session.Remove(key: key.ToLowerInvariant());

    public void AbortRequest(HttpContext context) =>
        context.Abort();

    public bool IsLocalUrl(IUrlHelper urlHelper, string url) =>
        urlHelper.IsLocalUrl(url: url);
}