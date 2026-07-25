// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;


namespace cCoder.Core.Exposures.OData.Responses;

public class RawResult(string response) : IActionResult
{
    private readonly string response = response;

    public Task ExecuteResultAsync(ActionContext context) =>
        Task.FromResult(
result: new HttpResponseMessage
{
    Content = new StringContent(response),
    StatusCode = System.Net.HttpStatusCode.OK,
}
        );
}