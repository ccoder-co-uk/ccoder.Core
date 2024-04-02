using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Services;

namespace Core.Api.Controllers
{
    public class PageInfoController : CoreEntityODataController<PageInfo, int>
    {
        protected new ICoreService<PageInfo> Service => 
            base.Service as ICoreService<PageInfo>;

        public PageInfoController(ICoreService<PageInfo> service, ICoreAuthInfo auth, ILogger<PageInfoController> log) 
            : base(service, auth, log) { }
    }
}