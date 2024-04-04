using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class PageInfoController : CoreEntityODataController<PageInfo, int>
    {
        protected new ICoreService<PageInfo> Service => 
            base.Service as ICoreService<PageInfo>;

        public PageInfoController(ICoreService<PageInfo> service, ICoreAuthInfo auth, ILogger<PageInfoController> log) 
            : base(service, auth, log) { }
    }
}