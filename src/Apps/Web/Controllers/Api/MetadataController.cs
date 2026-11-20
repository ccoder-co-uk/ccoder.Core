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
public sealed class MetadataController(
    IApiCacheAggregationService apiCacheAggregationService,
    ILoggingBroker loggingBroker)
    : Controller, ICompositionExposure
{
    [HttpGet("GetMetadata")]
    public IActionResult GetMetadata(
        string culture = "")
    {
        try
        {
            string metadata = apiCacheAggregationService.GetMetadata(culture: culture);

            loggingBroker.LogInformation(
                message: "The metadata was loaded successfully.");


            return Content(
                content: metadata,
                contentType: "application/json");
        }
        catch (ApiCacheValidationException exception)
        {
            loggingBroker.LogWarning(
                    exception: exception,
                    message: "The metadata request is invalid.");


            return BadRequest(error: "The metadata request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                    exception: exception,
                    message: "The metadata request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                    exception: exception,
                    message: "The metadata operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The metadata operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                    exception: exception,
                    message: "The metadata operation failed.");


            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The metadata could not be loaded.");
        }
    }
}