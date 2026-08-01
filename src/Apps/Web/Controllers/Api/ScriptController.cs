// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Web.Exposures;
using Web.Models;
using Web.Models.Exceptions;

namespace Web.Controllers.Api;

[Route("Api")]
public sealed class ScriptController(
    IApiScriptManager apiScriptManager)
    : Controller
{
    [HttpPost("ExecuteScript")]
    public async Task<IActionResult> PostExecuteScript()
    {
        try
        {
            using StreamReader reader = new(
                stream: Request.Body);

            ApiScriptRequest request = new()
            {
                Script = await reader.ReadToEndAsync()
            };

            string response =
                await apiScriptManager.ExecuteApiScriptRequestAsync(
                    request: request);

            return Ok(value: response);
        }
        catch (ApiScriptOrchestrationValidationException)
        {
            return BadRequest(error: "The script request is invalid.");
        }
        catch (Exception)
        {
            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The script could not be executed.");
        }
    }
}