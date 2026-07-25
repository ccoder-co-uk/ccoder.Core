// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Services.Processings;

internal interface IHomeSessionProcessingService
{
    bool CanUseSession(
        HttpContext context);

    string GetSessionValue(
        HttpContext context,
        string key);

    void SetSessionValue(
        HttpContext context,
        string key,
        string value);
}