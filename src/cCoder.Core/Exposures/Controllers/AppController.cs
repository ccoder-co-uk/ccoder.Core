using cCoder.Core.Models;
using cCoder.Core.Services.Orchestrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace cCoder.Core.Exposures.Controllers;

public class AppController(
    IAppOrchestrationService service) : ODataController
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

    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        await service.DeleteAsync(key);
        return Ok();
    }
}

