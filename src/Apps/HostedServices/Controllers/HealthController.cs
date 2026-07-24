// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("Health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Content(content: "OK",contentType: "text/plain");
}