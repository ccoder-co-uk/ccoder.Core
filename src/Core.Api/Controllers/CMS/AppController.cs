using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;

namespace Core.Api.Controllers
{
    public class AppController : CoreEntityODataController<App, int>
    {
        protected new IAppService Service => base.Service as IAppService;

        public AppController(IAppService service, ICoreAuthInfo auth, ILogger<AppController> log) 
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
        public IActionResult Export([FromRoute] int key, [FromQuery] string[] packageNames = null) => Ok(Service.Export(key, packageNames));

        [HttpGet]
        public IActionResult IsAdmin([FromRoute] int key, string userName) => Ok(Service.IsAdmin(key, userName));

        [HttpGet]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.All,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 6,
            MaxExpansionDepth = 6
        )]
        public IActionResult Users([FromRoute] int key) => Ok(Service.GetAppUsers(key));

        [HttpPost]
        public async Task<IActionResult> UpdatePageOrder([FromRoute] int key, ODataActionParameters p)
        {
            App app = p["app"] as App;
            await Service.UpdatePageOrder(key, app);
            return Ok();
        }
    }
}