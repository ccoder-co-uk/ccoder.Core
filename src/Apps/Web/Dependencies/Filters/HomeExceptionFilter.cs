// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures;
using cCoder.ContentManagement.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Web.Exposures;

namespace Web.Dependencies.Filters;

internal sealed class HomeExceptionFilter(
    IPageRenderer pageRenderer,
    IHomeSessionManager homeSessionManager,
    ILogger<HomeExceptionFilter> log)
    : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(
        ExceptionContext context)
    {
        Exception exception = context.Exception;
        HttpContext httpContext = context.HttpContext;

        log.LogWarning(
            exception: exception,
            message: "A page request failed.");

        try
        {
            string host = httpContext.Request.Host.Host
                .Replace(
                    oldValue: "www.",
                    newValue: string.Empty)
                .ToLowerInvariant();

            string theme =
                homeSessionManager.GetSessionValue(
                    context: httpContext,
                    key: "theme");

            string culture =
                homeSessionManager.GetSessionValue(
                    context: httpContext,
                    key: "culture");

            PageRenderResponse response =
                await pageRenderer.RenderErrorAsync(
                    request: new PageRenderRequest
                    {
                        Host = host,
                        Theme = theme,
                        Culture = culture,
                        RequestUrl =
                            httpContext.Request.GetEncodedUrl(),
                        Exception = exception
                    });

            context.Result = new ViewResult
            {
                ViewName = "Index",
                ViewData = CreateViewData(
                    model: response.Page)
            };
        }
        catch
        {
            context.Result = new PartialViewResult
            {
                ViewName = "Error",
                ViewData = CreateViewData(
                    model: exception)
            };
        }

        context.ExceptionHandled = true;
    }

    private static ViewDataDictionary CreateViewData(
        object model) =>
        new(
            metadataProvider:
                new EmptyModelMetadataProvider(),
            modelState: new ModelStateDictionary())
        {
            Model = model
        };
}