using Core.Objects;
using Core.Objects.Entities.Security;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using System.Security;

namespace Core.Api.Controllers
{
    public class FolderRoleController : CoreJoinEntityOdataController<FolderRole, Guid, Guid>
    {
        public FolderRoleController(ICoreService<FolderRole> service, ICoreAuthInfo auth, ILogger<FolderRoleController> log) 
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
        public virtual IActionResult Get([FromRoute] Guid keyFolderId, [FromRoute] Guid keyRoleId)
        {
            IQueryable<FolderRole> result = Service.GetAll().Where(i => i.FolderId == keyFolderId && i.RoleId == keyRoleId).AsQueryable();
            return !result.Any() ? throw new SecurityException("Access Denied!") : (IActionResult)Ok(SingleResult.Create(result));
        }

        [HttpDelete]
        public virtual async Task<IActionResult> Delete([FromRoute] Guid keyFolderId, [FromRoute] Guid keyRoleId)
        {
            await Service.DeleteAsync(BuildKey(keyFolderId, keyRoleId));
            return Ok();
        }
    }
}