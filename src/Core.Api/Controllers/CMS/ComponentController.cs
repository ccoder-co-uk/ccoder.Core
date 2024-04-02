using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers
{
    public class ComponentController : CoreEntityODataController<Component, int>
    {
        protected new IComponentService Service => base.Service as IComponentService;

        public ComponentController(IComponentService service, ICoreAuthInfo auth, ILogger<ComponentController> log) 
            : base(service, auth, log) { }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Render(int appId, string name, string culture, string theme) => 
            Ok(Service.Render(appId, name, culture, theme));
    }
}