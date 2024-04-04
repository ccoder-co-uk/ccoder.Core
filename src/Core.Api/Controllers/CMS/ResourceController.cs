using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.Core.Api.Controllers
{
    public class ResourceController : CoreEntityODataController<Resource, int>
    {
        protected new IResourceService Service => 
            base.Service as IResourceService;

        public ResourceController(IResourceService service, ICoreAuthInfo auth, ILogger<ResourceController> log) 
            : base(service, auth, log) { }

        [HttpGet]
        public IActionResult GetAll(int appId, string resourceKey, string culture) => 
            Ok(Service.GetAll(resourceKey, string.IsNullOrEmpty(culture) ? string.Empty : culture, appId));
    }
}