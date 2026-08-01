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
            try
            {
                var result = new
                {
                    value = apiContextManager.GetApiInfos()
                };

                return Ok(value: result);
            }
            catch (Exception)
            {
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: "The API contexts could not be loaded.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                using StreamReader reader = new(Request.Body);
                string response = await reader.ReadToEndAsync();

                return new RawResult(response);
            }
            catch (Exception)
            {
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: "The API request could not be processed.");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put()
        {
            try
            {
                return await Post();
            }
            catch (Exception)
            {
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: "The API request could not be processed.");
            }
        }

        [HttpGet("Time")]
        public IActionResult GetTime()
        {
            try
            {
                return Ok(value: new { DateTimeOffset.UtcNow });
            }
            catch (Exception)
            {
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: "The server time could not be loaded.");
            }
        }

    }
}