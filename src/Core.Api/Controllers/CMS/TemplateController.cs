using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.Core.Api.Controllers
{
    public class TemplateController : CoreEntityODataController<Template, int>
    {
        protected new ITemplateService Service => 
            base.Service as ITemplateService;

        public TemplateController(ITemplateService service, ICoreAuthInfo auth, ILogger<TemplateController> log) 
            : base(service, auth, log) { }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Render(int appId, string name, string culture, [FromBody] dynamic model) => 
            Ok(Service.Render(appId, name, culture, model));
    }
}