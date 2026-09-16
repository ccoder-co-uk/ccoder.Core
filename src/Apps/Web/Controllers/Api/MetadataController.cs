// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Exposures;
using Web.Models.Exceptions;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class MetadataController(
    IMetadataManager metadataManager)
    : Controller
{
    [HttpGet("GetMetadata")]
    public IActionResult GetMetadata(
        string culture = "")
    {
        try
        {
            return Content(
                content: metadataManager.GetAll(
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