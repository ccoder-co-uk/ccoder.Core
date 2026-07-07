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
            return BadRequest(ModelState);

        return Ok(await service.AddAsync(entity));
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] App entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        entity.Id = key;
        return Ok(await service.UpdateAsync(entity));
    }

    [ODataIgnored]
    [HttpPut("Api/Core/App({key})", Order = -1)]
    public Task<IActionResult> PutAggregateRoute([FromRoute] int key, [FromBody] App entity) =>
        Put(key, entity);

    [ODataIgnored]
    [HttpDelete("Api/Core/App({key})", Order = -1)]
    public Task<IActionResult> DeleteAggregateRoute([FromRoute] int key) =>
        Delete(key);

    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        if (IsExternalEventingEnabled())
        {
            await DeleteViaExternalEventingAsync(key);
            return Ok();
        }

        await service.DeleteAsync(key);
        return Ok();
    }

    private ValueTask DeleteViaExternalEventingAsync(int key) =>
        contentManagementAppService.DeleteAsync(key);

    private bool IsExternalEventingEnabled() =>
        configuration.EnableHttpEventing || configuration.EnableServiceBusEventing;
}

