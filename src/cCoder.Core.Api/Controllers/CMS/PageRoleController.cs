using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using System.Security;

namespace cCoder.Core.Api.Controllers.CMS;

public class PageRoleController : CoreJoinEntityOdataController<PageRole, int, Guid>
{
    protected new ICoreService<PageRole> Service =>
        base.Service as ICoreService<PageRole>;

    public PageRoleController(ICoreService<PageRole> service, ICoreAuthInfo auth, ILogger<PageRoleController> log)
        : base(service, auth, log) { }

    [HttpGet]
    [EnableQuery(
        AllowedArithmeticOperators = AllowedArithmeticOperators.All,
        AllowedFunctions = AllowedFunctions.AllFunctions,
        AllowedLogicalOperators = AllowedLogicalOperators.All,
        AllowedQueryOptions = AllowedQueryOptions.All,
        MaxAnyAllExpressionDepth = 3,
        MaxExpansionDepth = 3
    )]
    public virtual IActionResult Get([FromRoute] int keyPageId, [FromRoute] Guid keyRoleId)
    {
        IQueryable<PageRole> result = Service.GetAll()
            .Where(i => i.PageId == keyPageId && i.RoleId == keyRoleId)
            .AsQueryable();

        return !result.Any()
            ? throw new SecurityException("Access Denied!")
            : (IActionResult)Ok(SingleResult.Create(result));
    }

    [HttpDelete]
    public virtual async Task<IActionResult> Delete([FromRoute] int keyPageId, [FromRoute] Guid keyRoleId)
    {
        await Service.DeleteAsync(BuildKey(keyPageId, keyRoleId));
        return Ok();
    }
}