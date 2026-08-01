// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("")]
public sealed class HomeController() : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            Response.StatusCode = StatusCodes.Status200OK;

            return View(viewName: "Index");
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The home page could not be displayed.");
        }
    }
}