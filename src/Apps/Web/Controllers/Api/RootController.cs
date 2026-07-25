// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Exposures.OData.Responses;
using cCoder.Data;
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
        public IActionResult Get()
        {
            var result = new
            {
                value = apiContextManager.GetApiInfos()
            };

            return Ok(value: result);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using StreamReader reader = new(Request.Body);
            string response = await reader.ReadToEndAsync();
            return new RawResult(response);
        }

        [HttpPut]
        public Task<IActionResult> Put() =>
            Post();

        [HttpGet("Time")]
        public IActionResult GetTime() =>
            Ok(value: new { DateTimeOffset.UtcNow });

    }
}