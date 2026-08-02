// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures;
using cCoder.Core.Exposures;
using Microsoft.AspNetCore.Mvc.Filters;
using App = cCoder.Data.Models.CMS.App;

namespace Web.Dependencies.Filters;

internal sealed class HomeDefaultsActionFilter(
    IAppManager appProcessingService,
    ILogger<HomeDefaultsActionFilter> log)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        try
        {
            string host = context.HttpContext.Request.Host.Host
                .Replace(
                    oldValue: "www.",
                    newValue: string.Empty)
                .ToLowerInvariant();

            App app = appProcessingService
                .GetAll(ignoreFilters: true)
                .Where(predicate: candidate =>
                    candidate.Domain == host)
                .Select(selector: candidate => new App
                {
                    Id = candidate.Id,
                    Domain = candidate.Domain,
                    DefaultCultureId =
                        candidate.DefaultCultureId,
                    DefaultTheme = candidate.DefaultTheme
                })
                .FirstOrDefault();

            if (app is not null
                && GetSessionValue(
                    context: context.HttpContext,
                    key: "theme") is null)
            {
                SetSessionValue(
                    context: context.HttpContext,
                    key: "theme",
                    value: app.DefaultTheme ?? "Default");
            }

            if (app is not null
                && GetSessionValue(
                    context: context.HttpContext,
                    key: "culture") is null)
            {
                SetSessionValue(
                    context: context.HttpContext,
                    key: "culture",
                    value: app.DefaultCultureId
                        ?? string.Empty);
            }
        }
        catch (Exception exception)
        {
            log.LogWarning(
                exception: exception,
                message:
                    "Unable to determine the current app domain and set request defaults.");
        }

        await next();
    }

    private static string GetSessionValue(
        HttpContext context,
        string key) =>
        CanUseSession(context: context)
        && context.Session.Keys.Contains(
            value: key.ToLowerInvariant())
                ? context.Session.GetString(key: key)
                : null;

    private static void SetSessionValue(
        HttpContext context,
        string key,
        string value)
    {
        if (!CanUseSession(context: context))
        {
            return;
        }

        if (value is not null)
        {
            context.Session.SetString(
                key: key.ToLowerInvariant(),
                value: value);
        }
        else if (context.Session.Keys.Contains(
            value: key.ToLowerInvariant()))
        {
            context.Session.Remove(
                key: key.ToLowerInvariant());
        }
    }

    private static bool CanUseSession(
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