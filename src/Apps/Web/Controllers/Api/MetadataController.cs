// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Services.Aggregations;
using Web.Models.Exceptions;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class MetadataController(
    IApiCacheAggregationService metadataManager)
    : Controller
{
    [HttpGet("GetMetadata")]
    public IActionResult GetMetadata(
        string culture = "")
    {
        try
        {
            return Content(
                content: metadataManager.GetMetadata(
                    culture: culture),
                contentType: "application/json");
        }
        catch (ApiCacheValidationException exception)
        {
            metadataManager.LogError(exception: exception);

            return BadRequest(error: "The metadata request is invalid.");
        }
        catch (Exception exception)
        {
            metadataManager.LogError(exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The metadata could not be loaded.");
        }
    }
}