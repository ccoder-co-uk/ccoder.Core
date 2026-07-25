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
        string culture = "") =>
        Content(
            content: metadataCache.GetAll(
                culture: culture),
            contentType: "application/json");
}