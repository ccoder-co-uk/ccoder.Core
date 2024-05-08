using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;

namespace cCoder.Core.Api.Controllers.CMS;

public class AppCultureController : CoreJoinEntityOdataController<AppCulture, int, string>
{
    protected new ICoreService<AppCulture> Service =>
        base.Service as ICoreService<AppCulture>;

    public AppCultureController(ICoreService<AppCulture> service, ICoreAuthInfo auth, ILogger<AppCultureController> log)
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
    public virtual IActionResult Get([FromRoute] int keyAppId, [FromRoute] string keyCultureId)
    {
        IQueryable<AppCulture> result = Service.GetAll()
            .Where(i => i.AppId == keyAppId && i.CultureId == keyCultureId)
            .AsQueryable();

        return Ok(SingleResult.Create(result));
    }

    [HttpDelete]
    public virtual async Task<IActionResult> Delete([FromRoute] int keyAppId, [FromRoute] string keyCultureId)
    {
        await Service.DeleteAsync(BuildKey(keyAppId, keyCultureId));
        return Ok();
    }
}