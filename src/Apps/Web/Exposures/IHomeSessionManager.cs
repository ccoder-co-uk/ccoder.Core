// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Dynamic;

namespace Web.Exposures;

public interface IHomeSessionManager
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