// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace Web.Services.Processings;

internal interface IHomeSessionProcessingService
{
    bool CanUseSession(
        HttpContext context);

    ExpandoObject CreateExpandoObject(
        HttpContext context);

    string GetSessionValue(
        HttpContext context,
        string key);

    void SetSessionValue(
        HttpContext context,
        string key,
        string value);

    void AbortRequest(HttpContext context);

    bool IsLocalUrl(IUrlHelper urlHelper, string url);
}