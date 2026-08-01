// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Models.Exceptions;
using cCoder.Core.Exposures.Managers;
using cCoder.Mail.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.Core.Exposures.Controllers;

[ApiController]
public class TemplatedEmailController(
    ITemplatedEmailManager templatedEmailOrchestrationService) : ControllerBase
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
                        details: newTemplatedEmailDetails));
        }
        catch (CoreOrchestrationValidationException)
        {
            return BadRequest(error: "The email request is invalid.");
        }
        catch (System.Security.SecurityException)
        {
            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The email operation is forbidden.");
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The email operation failed.");
        }
    }
}