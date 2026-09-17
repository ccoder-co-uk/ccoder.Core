// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Services.Orchestrations.Api;
using Web.Models;
using Web.Models.Exceptions;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class ScriptController(
    IApiScriptOrchestrationService apiScriptOrchestrationService)
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

            return Ok(value: response);
        }
        catch (ApiScriptOrchestrationValidationException exception)
        {
            apiScriptOrchestrationService.LogError(exception: exception);

            return BadRequest(error: "The script request is invalid.");
        }
        catch (Exception exception)
        {
            apiScriptOrchestrationService.LogError(exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The script could not be executed.");
        }
    }
}