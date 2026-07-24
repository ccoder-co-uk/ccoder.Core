// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace cCoder.Core.Exposures.Controllers;

public class AppController(
    IAppOrchestrationService service,
    IContentManagementAppService contentManagementAppService,
    CoreConfiguration configuration) : ODataController
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] App entity)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(modelState: ModelState);
        }

        return Ok(value: await service.AddAppAsync(newApp: entity));
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] App entity)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(modelState: ModelState);
        }

        entity.Id = key;
        return Ok(value: await service.UpdateAppAsync(updatedApp: entity));
    }

    [ODataIgnored]
    [HttpPut("Api/Core/App({key})", Order = -1)]
    public Task<IActionResult> PutAggregateRoute([FromRoute] int key, [FromBody] App entity) =>
        Put(key: key, entity: entity);

    [ODataIgnored]
    [HttpDelete("Api/Core/App({key})", Order = -1)]
    public Task<IActionResult> DeleteAggregateRoute([FromRoute] int key) =>
        Delete(key: key);

    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        if (IsExternalEventingEnabled())
        {
            await DeleteViaExternalEventingAsync(key: key);
            return Ok();
        }

        await service.DeleteAppAsync(appId: key);
        return Ok();
    }

    private ValueTask DeleteViaExternalEventingAsync(int key) =>
        contentManagementAppService.DeleteAppAsync(appId: key);

    private bool IsExternalEventingEnabled() =>
        configuration.EnableHttpEventing || configuration.EnableServiceBusEventing;
}