using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Services;

namespace Core.Api.Controllers
{
    public class LayoutController : CoreEntityODataController<Layout, int>
    {
        public LayoutController(ICoreService<Layout> service, ICoreAuthInfo auth, ILogger<LayoutController> log) 
            : base(service, auth, log) { }
    }
}