// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Services.Processings;

namespace Web.Exposures;

internal sealed class HomeSessionManager(
    IHomeSessionProcessingService homeSessionProcessingService)
    : IHomeSessionManager
{
    public bool CanUseSession(
        HttpContext context) =>
        homeSessionProcessingService.CanUseSession(
            context: context);

    public string GetSessionValue(
        HttpContext context,
        string key) =>
        homeSessionProcessingService.GetSessionValue(
            context: context,
            key: key);

    public void SetSessionValue(
        HttpContext context,
        string key,
        string value) =>
        homeSessionProcessingService.SetSessionValue(
            context: context,
            key: key,
            value: value);
}