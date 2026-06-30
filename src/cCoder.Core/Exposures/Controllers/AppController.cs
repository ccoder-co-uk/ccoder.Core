using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.CMS;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPut("Api/Core/App({key})", Order = -1)]
    public Task<IActionResult> PutAggregateRoute([FromRoute] int key, [FromBody] App entity) =>
        Put(key, entity);

    [HttpDelete("Api/Core/App({key})", Order = -1)]
    public Task<IActionResult> DeleteAggregateRoute([FromRoute] int key) =>
        IsExternalEventingEnabled()
            ? DeleteViaExternalEventingAsync(key)
            : Delete(key);

    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        await service.DeleteAsync(key);
        return Ok();
    }

    private async Task<IActionResult> DeleteViaExternalEventingAsync(int key)
    {
        await contentManagementAppService.DeleteAsync(key);
        return Ok();
    }

    private bool IsExternalEventingEnabled() =>
        configuration.EnableHttpEventing || configuration.EnableServiceBusEventing;
}

