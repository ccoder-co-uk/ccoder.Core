using Core.Objects;
using Core.Objects.Entities.Planning;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using System.Security;

namespace Core.Api.Controllers
{
    public class CalendarController : CoreEntityODataController<Calendar, int>
    {
        public CalendarController(ICoreService<Calendar> service, ICoreAuthInfo auth, ILogger<CalendarController> log) : base(service, auth, log) { }

        [HttpGet]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.AllFunctions,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 3,
            MaxExpansionDepth = 3
            )]
        public override IActionResult Get([FromRoute] int key)
        {
            IQueryable<Calendar> result = Service.GetAll().Where(i => i.Id == key).AsQueryable();
            return !result.Any() ? throw new SecurityException("Access Denied!") : (IActionResult)Ok(SingleResult.Create(result));
        }
    }
}