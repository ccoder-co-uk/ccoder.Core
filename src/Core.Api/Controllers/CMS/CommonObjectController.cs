using Core.Objects;
using Core.Objects.Dtos;
using Core.Objects.Entities;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Core.Api.Controllers
{
    public class CommonObjectController : CoreEntityODataController<CommonObject, int>
    {
        protected new ICommonObjectService Service => 
            base.Service as ICommonObjectService;

        public CommonObjectController(ICommonObjectService service, ICoreAuthInfo auth, ILogger<CommonObjectController> log) 
            : base(service, auth, log) { }

        [HttpGet]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.All,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 6,
            MaxExpansionDepth = 6
        )]
        public IActionResult Latest(string type) => Ok(Service.Latest(type));

        [HttpPost]
        public async Task<IActionResult> Import([FromBody] ODataCollection<CommonObject> items) => 
            ModelState.IsValid 
                ? Ok(await Service.Import(items.Value)) 
                : BadRequest(ModelState);

    }
}