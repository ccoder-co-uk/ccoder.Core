// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using cCoder.Core.Brokers.Loggings;
using cCoder.CodeAnalysis.Exposures;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Models.Exceptions;
using cCoder.Data.Models.CMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace cCoder.Core.Exposures.Controllers;

public class AppController(
    IAppAggregationService service,
    ILoggingBroker loggingBroker)
    : ODataController, ICompositionExposure
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] App newApp)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                loggingBroker.LogWarning(
                    message: "The app request is invalid.");

                return BadRequest(modelState: ModelState);
            }

            App savedApp = await service.AddAppAsync(newApp: newApp);

            loggingBroker.LogInformation(
                message: "The app was created successfully.");

            return StatusCode(
                statusCode: StatusCodes.Status201Created,
                value: savedApp);
        }
        catch (CoreOrchestrationValidationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app request is invalid.");

            return BadRequest(error: "The app request is invalid.");
        }
        catch (DbUpdateConcurrencyException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app changed before completion.");

            return Conflict(error: "The app changed before the request completed.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The app operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "The app operation failed.");

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
                loggingBroker.LogWarning(message: "The app request is invalid.");
                return BadRequest(modelState: ModelState);
            }

            updatedApp.Id = key;

            App savedApp = await service.UpdateAppAsync(updatedApp: updatedApp);

            loggingBroker.LogInformation(message: "The app was updated successfully.");

            return Ok(value: savedApp);
        }
        catch (CoreOrchestrationValidationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app request is invalid.");

            return BadRequest(error: "The app request is invalid.");
        }
        catch (DbUpdateConcurrencyException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app changed before completion.");

            return Conflict(error: "The app changed before the request completed.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The app operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "The app operation failed.");

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
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The app operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "The app operation failed.");

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
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The app operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "The app operation failed.");

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

            loggingBroker.LogInformation(message: "The app was deleted successfully.");

            return accepted
                ? Accepted()
                : NoContent();
        }
        catch (CoreOrchestrationValidationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app request is invalid.");

            return BadRequest(error: "The app request is invalid.");
        }
        catch (DbUpdateConcurrencyException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app changed before completion.");

            return Conflict(error: "The app changed before the request completed.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app request was not authenticated.");

            return Unauthorized(value: "Authentication failed.");
        }
        catch (System.Security.SecurityException exception)
        {
            loggingBroker.LogWarning(
                exception: exception,
                message: "The app operation is forbidden.");

            return StatusCode(
                statusCode: StatusCodes.Status403Forbidden,
                value: "The app operation is forbidden.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(
                exception: exception,
                message: "The app operation failed.");

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: "The app operation failed.");
        }
    }
}