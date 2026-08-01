// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using ContentManagementMetadataCache =
    cCoder.ContentManagement.Exposures.Caching.IMetadataCache;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class MetadataController(
    ContentManagementMetadataCache metadataCache)
    : Controller
{
    [HttpGet("GetMetadata")]
    public IActionResult GetMetadata(
        string culture = "")
    {
        try
        {
            return Content(
                content: metadataCache.GetAll(
                    culture: culture),
                contentType: "application/json");
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The metadata could not be loaded.");
        }
    }
}