// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Models.Exceptions;
using cCoder.Mail.Models;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.Core.Exposures.Controllers;

[ApiController]
public class TemplatedEmailController(
    ITemplatedEmailOrchestrationService templatedEmailOrchestrationService,
    ILoggingBroker loggingBroker) : ControllerBase
{
    [HttpPost("Api/Core/QueuedEmail/AddTemplatedEmail()")]
    public async Task<IActionResult> Post(
        [FromBody] TemplatedEmailDetails newTemplatedEmailDetails)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                loggingBroker.LogWarning(
                    message: "The email request is invalid.");

                return BadRequest(modelState: ModelState);
            }

            QueuedEmail queuedEmail = await templatedEmailOrchestrationService
                .QueueTemplatedEmailDetailsAsync(
                    templatedEmailDetails: newTemplatedEmailDetails);

            loggingBroker.LogInformation(
                message: "The email request completed successfully.");

            return Ok(value: queuedEmail);
        }
        catch (CoreOrchestrationValidationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The email request is invalid.");

            return BadRequest(error: "The email request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The email request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The email operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The email operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "The email operation failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The email operation failed.");
        }
    }
}