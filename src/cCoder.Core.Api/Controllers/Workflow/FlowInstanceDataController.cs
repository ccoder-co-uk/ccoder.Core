using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace cCoder.Core.Api.Controllers.Workflow;

public class FlowInstanceDataController : CoreEntityODataController<FlowInstanceData, Guid>
{
    public FlowInstanceDataController(ICoreService<FlowInstanceData> service, ICoreAuthInfo auth, ILogger<FlowInstanceDataController> log)
        : base(service, auth, log) { }

    [HttpPost]
    [DisableRequestSizeLimit]
    [HttpPut]
    [EnableQuery(
        AllowedArithmeticOperators = AllowedArithmeticOperators.All,
        AllowedFunctions = AllowedFunctions.AllFunctions,
        AllowedLogicalOperators = AllowedLogicalOperators.All,
        AllowedQueryOptions = AllowedQueryOptions.All,
        MaxAnyAllExpressionDepth = 3,
        MaxExpansionDepth = 3
    )]
    public override async Task<IActionResult> Put([FromRoute] Guid key, [FromBody] FlowInstanceData entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        else
        {
            await Service.UpdateAsync(entity);
            return NoContent();
        }
    }
}