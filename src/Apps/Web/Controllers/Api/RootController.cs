// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Exposures.OData.Responses;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Foundations.Api;


namespace Web.Controllers.Api
{
    [Route("Api")]
    public class ApiRootController(
        IApiContextService apiContextService,
        ILoggingBroker loggingBroker)
        : Controller
    {
        [HttpGet()]
        public IActionResult Get()
        {
            try
            {
                object value = new
                {
                    value = apiContextService.GetApiInfos()
                };

                loggingBroker.LogInformation(message: "The API contexts were loaded successfully.");

                return Ok(value: value);
            }
            catch (AuthenticationException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The API request was not authenticated.");

                return Unauthorized(value: "Authentication failed.");
            }
            catch (System.Security.SecurityException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The API request was forbidden.");

                return StatusCode(
                    statusCode: StatusCodes.Status403Forbidden,
                    value: "The API request is forbidden.");
            }
            catch (Exception exception)
            {
                loggingBroker.LogError(
                    exception: exception,
                    message: "The API contexts could not be loaded.");


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
                string response =
                    await apiContextService.ReadRequestBodyAsync(
                        requestBody: Request.Body);

                loggingBroker.LogInformation(message: "The API request completed successfully.");

                return new RawResult(response);
            }
            catch (AuthenticationException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The API request was not authenticated.");

                return Unauthorized(value: "Authentication failed.");
            }
            catch (System.Security.SecurityException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The API request was forbidden.");

                return StatusCode(
                    statusCode: StatusCodes.Status403Forbidden,
                    value: "The API request is forbidden.");
            }
            catch (Exception exception)
            {
                loggingBroker.LogError(
                    exception: exception,
                    message: "The API request could not be processed.");


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
            catch (AuthenticationException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The API request was not authenticated.");

                return Unauthorized(value: "Authentication failed.");
            }
            catch (System.Security.SecurityException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The API request was forbidden.");

                return StatusCode(
                    statusCode: StatusCodes.Status403Forbidden,
                    value: "The API request is forbidden.");
            }
            catch (Exception exception)
            {
                loggingBroker.LogError(
                    exception: exception,
                    message: "The API request could not be processed.");


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
                object value = new { DateTimeOffset.UtcNow };

                loggingBroker.LogInformation(message: "The server time was loaded successfully.");

                return Ok(value: value);
            }
            catch (AuthenticationException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The API request was not authenticated.");

                return Unauthorized(value: "Authentication failed.");
            }
            catch (System.Security.SecurityException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The API request was forbidden.");

                return StatusCode(
                    statusCode: StatusCodes.Status403Forbidden,
                    value: "The API request is forbidden.");
            }
            catch (Exception exception)
            {
                loggingBroker.LogError(
                    exception: exception,
                    message: "The server time could not be loaded.");


                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: "The server time could not be loaded.");
            }
        }

    }
}