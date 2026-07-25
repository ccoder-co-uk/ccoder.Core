// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Dynamic;

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
}