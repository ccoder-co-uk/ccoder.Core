using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using System.Security;

namespace cCoder.Core.Api.Controllers.Security;

public class UserRoleController : CoreJoinEntityOdataController<UserRole, string, Guid>
{
    protected new IUserRoleService Service =>
        base.Service as IUserRoleService;

    public UserRoleController(IUserRoleService service, ICoreAuthInfo auth, ILogger<UserRoleController> log)
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
    public virtual IActionResult Get([FromRoute] string keyUserId, [FromRoute] Guid keyRoleId)
    {
        IQueryable<UserRole> result = Service.GetAll()
            .Where(i => i.UserId == keyUserId && i.RoleId == keyRoleId)
            .AsQueryable();

        return !result.Any()
            ? throw new SecurityException("Access Denied!")
            : (IActionResult)Ok(SingleResult.Create(result));
    }


    [HttpDelete]
    public virtual async Task<IActionResult> Delete([FromRoute] string keyUserId, [FromRoute] Guid keyRoleId)
    {
        await Service.DeleteAsync(BuildKey(keyUserId, keyRoleId));
        return Ok();
    }
}