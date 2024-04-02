using Core.Objects;
using Core.Objects.Entities.Packaging;
using Core.Services;

namespace Core.Api.Controllers
{
    public class PackageItemController : CoreEntityODataController<PackageItem, Guid>
    {
        public PackageItemController(ICoreService<PackageItem> service, ICoreAuthInfo auth, ILogger<PackageItemController> log) 
            : base(service, auth, log) { }
    }
}