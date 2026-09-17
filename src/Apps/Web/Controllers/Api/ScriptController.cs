// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Models.Exceptions;
using Web.Services.Orchestrations.Api;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class ScriptController(
    IApiScriptOrchestrationService apiScriptManager)
    : Controller
{
    [HttpPost("ExecuteScript")]
    public async Task<IActionResult> PostExecuteScript()
    {
        try
        {
            ApiScriptRequest request = new()
            {
                Script = await apiScriptManager.ReadRequestBodyAsync(
                    requestBody: Request.Body)
            };

            string response =
                await apiScriptManager.ExecuteApiScriptRequestAsync(
                    apiScriptRequest: request);

            return Ok(value: response);
        }
        catch (ApiScriptOrchestrationValidationException exception)
        {
            apiScriptManager.LogError(exception: exception);

            return BadRequest(error: "The script request is invalid.");
        }
        catch (Exception exception)
        {
            apiScriptManager.LogError(exception: exception);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The script could not be executed.");
        }
    }
}