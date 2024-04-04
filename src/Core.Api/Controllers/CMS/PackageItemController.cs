using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class PackageItemController : CoreEntityODataController<PackageItem, Guid>
    {
        public PackageItemController(ICoreService<PackageItem> service, ICoreAuthInfo auth, ILogger<PackageItemController> log) 
            : base(service, auth, log) { }
    }
}