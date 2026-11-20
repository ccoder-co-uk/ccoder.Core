// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using cCoder.Core.Brokers.Loggings;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Orchestrations.Api;
using Web.Models;
using Web.Models.Exceptions;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class ScriptController(
    IApiScriptOrchestrationService apiScriptOrchestrationService,
    ILoggingBroker loggingBroker)
    : Controller
{
    [HttpPost("ExecuteScript")]
    public async Task<IActionResult> PostExecuteScript()
    {
        try
        {
            ApiScriptRequest request = new()
            {
                Script = await apiScriptOrchestrationService.ReadRequestBodyAsync(
                    requestBody: Request.Body)
            };

            string response =
                await apiScriptOrchestrationService.ExecuteApiScriptRequestAsync(
                    apiScriptRequest: request);

            loggingBroker.LogInformation(
                message: "The script request completed successfully.");


            return Ok(value: response);
        }
        catch (ApiScriptOrchestrationValidationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The script request is invalid.");


            return BadRequest(error: "The script request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                    exception: exception,
                    message: "The script request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                    exception: exception,
                    message: "The script operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The script operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                    exception: exception,
                    message: "The script operation failed.");


            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The script could not be executed.");
        }
    }
}