// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Authentication;
using cCoder.CodeAnalysis.Exposures;
using cCoder.Core.Brokers.Loggings;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Aggregations;
using Web.Models.Exceptions;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class CacheController(
    IApiCacheAggregationService apiCacheAggregationService,
    ILoggingBroker loggingBroker)
    : Controller, ICompositionExposure
{
    [HttpGet("RefreshCache")]
    public IActionResult GetRefreshCache()
    {
        try
        {
            apiCacheAggregationService.RefreshCaches();

            loggingBroker.LogInformation(
                message: "The caches were refreshed successfully.");


            return Ok();
        }
        catch (ApiCacheValidationException exception)
        {
            loggingBroker.LogWarning(
                    exception: exception,
                    message: "The cache refresh request is invalid.");


            return BadRequest(error: "The cache refresh request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                    exception: exception,
                    message: "The cache request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                    exception: exception,
                    message: "The cache operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The cache operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                    exception: exception,
                    message: "The cache operation failed.");


            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The caches could not be refreshed.");
        }
    }
}