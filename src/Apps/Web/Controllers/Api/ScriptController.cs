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
}