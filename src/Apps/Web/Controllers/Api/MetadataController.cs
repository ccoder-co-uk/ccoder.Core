// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using Microsoft.AspNetCore.Mvc;
using ContentManagementMetadataCache =
    cCoder.ContentManagement.Exposures.Caching.IMetadataCache;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class MetadataController(
    ContentManagementMetadataCache metadataCache,
    ILoggingBroker loggingBroker)
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
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception, message: "Metadata loading failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The metadata could not be loaded.");
        }
    }
}