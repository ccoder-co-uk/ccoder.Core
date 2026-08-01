// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Exposures;
using Web.Models.Exceptions;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class CacheController(
    IApiCacheManager apiCacheManager)
    : Controller
{
    [HttpGet("RefreshCache")]
    public IActionResult GetRefreshCache()
    {
        try
        {
            apiCacheManager.RefreshCaches();

            return Ok();
        }
        catch (ApiCacheValidationException)
        {
            return BadRequest(error: "The cache refresh request is invalid.");
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The caches could not be refreshed.");
        }
    }
}