// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;


namespace cCoder.Core.Exposures.OData.Responses;

public class RawResult(string response) : IActionResult
{
    private readonly string response = response;

    public Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.StatusCode =
            StatusCodes.Status200OK;

        return context.HttpContext.Response.WriteAsync(
            text: response);
    }
}