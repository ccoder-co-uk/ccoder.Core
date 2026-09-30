// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Models.Exceptions;
using cCoder.Mail.Models;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.Core.Exposures.Controllers;

[ApiController]
public class TemplatedEmailController(
    ITemplatedEmailOrchestrationService templatedEmailOrchestrationService) : ControllerBase
{
    [HttpPost("Api/Core/QueuedEmail/AddTemplatedEmail()")]
    public async Task<IActionResult> Post(
        [FromBody] TemplatedEmailDetails newTemplatedEmailDetails)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(modelState: ModelState);
            }

            return Ok(
                value: await templatedEmailOrchestrationService
                    .QueueTemplatedEmailDetailsAsync(
                        templatedEmailDetails: newTemplatedEmailDetails));
        }
        catch (CoreOrchestrationValidationException exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return BadRequest(error: "The email request is invalid.");
        }
        catch (System.Security.SecurityException exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The email operation is forbidden.");
        }
        catch (Exception exception)
        {
            WebApplicationExtensions.LogCoreControllerException(
                context: HttpContext,
                exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The email operation failed.");
        }
    }
}