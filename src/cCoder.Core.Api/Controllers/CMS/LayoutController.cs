using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers.CMS;

public class LayoutController : CoreEntityODataController<Layout, int>
{
    public LayoutController(ICoreService<Layout> service, ICoreAuthInfo auth, ILogger<LayoutController> log)
        : base(service, auth, log) { }
}