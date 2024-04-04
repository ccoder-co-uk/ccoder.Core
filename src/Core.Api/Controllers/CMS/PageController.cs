using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace cCoder.Core.Api.Controllers
{
    public class PageController : CoreEntityODataController<Page, int>
    {
        protected new IPageService Service => 
            base.Service as IPageService;

        public PageController(IPageService service, ICoreAuthInfo auth, ILogger<PageController> log) 
            : base(service, auth, log) { }

        [HttpGet]
        [AllowAnonymous]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.All,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 6,
            MaxExpansionDepth = 6
        )]
        public override IActionResult Get(ODataQueryOptions<Page> queryOptions) => Ok(Service.GetAll());

        [HttpGet]
        [AllowAnonymous]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.All,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 6,
            MaxExpansionDepth = 6
        )]
        public IActionResult RootFor([FromRoute] int key) => Ok(Service.GetRoot(key));

        [HttpGet]
        public IActionResult Menu([FromRoute] int key, string culture) => Ok(new Result<string> { Id = key.ToString(), Item = Service.MenuFor(key, culture), Success = true });

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Render(int appId, string path, string theme, string culture) => Ok(Service.Render(appId, path, theme, culture));
    }
}