// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Exposures.OData.Responses;
using cCoder.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Web.Exposures;


namespace Web.Controllers.Api
{
    [Route("Api")]
    public class ApiRootController(
        IApiContextManager apiContextManager)
        : Controller
    {
    [HttpGet()]
        public IActionResult Get() =>
            Ok(value: new
            {
                value = apiContextManager.GetApiInfos()
            });

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            string response = await apiContextManager.ReadRequestBodyAsync(
                requestBody: Request.Body);

            return new RawResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> Put() =>
            await Post();

        [HttpGet("Time")]
        public IActionResult GetTime() =>
            Ok(value: new { DateTimeOffset.UtcNow });

    }
}