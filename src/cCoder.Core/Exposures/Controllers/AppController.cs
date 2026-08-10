// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Models.Exceptions;
using cCoder.Data.Models.CMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace cCoder.Core.Exposures.Controllers;

public class AppController(
    ICoreAppManager service,
    ILoggingBroker loggingBroker) : ODataController
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] App newApp)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(modelState: ModelState);
            }

            return StatusCode(
                statusCode: StatusCodes.Status201Created,
                value: await service.AddAppAsync(newApp: newApp));
        }
        catch (CoreOrchestrationValidationException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return BadRequest(error: "The app request is invalid.");
        }
        catch (DbUpdateConcurrencyException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return Conflict(error: "The app changed before the request completed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The app operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The app operation failed.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> Put(
        [FromRoute] int key,
        [FromBody] App updatedApp)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(modelState: ModelState);
            }

            updatedApp.Id = key;

            return Ok(
                value: await service.UpdateAppAsync(
                    updatedApp: updatedApp));
        }
        catch (CoreOrchestrationValidationException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return BadRequest(error: "The app request is invalid.");
        }
        catch (DbUpdateConcurrencyException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return Conflict(error: "The app changed before the request completed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The app operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The app operation failed.");
        }
    }

    [ODataIgnored]
    [HttpPut("Api/Core/App({key})", Order = -1)]
    public async Task<IActionResult> PutAggregateRoute(
        [FromRoute] int key,
        [FromBody] App updatedApp)
    {
        try
        {
            return await Put(key: key, updatedApp: updatedApp);
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The app operation failed.");
        }
    }

    [ODataIgnored]
    [HttpDelete("Api/Core/App({key})", Order = -1)]
    public async Task<IActionResult> DeleteAggregateRoute(
        [FromRoute] int key)
    {
        try
        {
            return await Delete(key: key);
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The app operation failed.");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        try
        {
            bool accepted = await service.DeleteAppAsync(appId: key);

            return accepted
                ? Accepted()
                : NoContent();
        }
        catch (CoreOrchestrationValidationException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return BadRequest(error: "The app request is invalid.");
        }
        catch (DbUpdateConcurrencyException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return Conflict(error: "The app changed before the request completed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The app operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception, message: "Controller request failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The app operation failed.");
        }
    }
}