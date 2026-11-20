// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Authentication;
using cCoder.Logging.Brokers.Loggings;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("")]
public sealed class HomeController(ILoggingBroker loggingBroker) : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            Response.StatusCode = StatusCodes.Status200OK;

            loggingBroker.LogInformation(message: "The home page was loaded successfully.");

            return View(viewName: "Index");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);
            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(message: exception.Message);
            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "The home page could not be displayed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The home page could not be displayed.");
        }
    }
}