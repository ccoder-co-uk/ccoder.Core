// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Exposures;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class CacheController(
    IApiCacheManager apiCacheManager)
    : Controller
{
    [HttpGet("RefreshCache")]
    public IActionResult GetRefreshCache()
    {
        apiCacheManager.RefreshCaches();

        return Ok();
    }
}