using Core.Objects;
using Core.Objects.Entities.Security;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Core.Api.Controllers
{
    public class UserController : CoreEntityODataController<User, string>
    {
        public new ICoreService<User> Service => 
            base.Service as ICoreService<User>;

        public UserController(ICoreService<User> service, ICoreAuthInfo auth, ILogger<UserController> log) 
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
        public IActionResult Me() => 
            Ok(Service.User);
    }
}