// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using Microsoft.AspNetCore.Mvc;
using Web.Exposures;
using Web.Models.Exceptions;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class CacheController(
    IApiCacheManager apiCacheManager,
    ILoggingBroker loggingBroker)
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
        catch (ApiCacheValidationException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Cache refresh validation failed.");

            return BadRequest(error: "The cache refresh request is invalid.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception, message: "Cache refresh failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The caches could not be refreshed.");
        }
    }
}