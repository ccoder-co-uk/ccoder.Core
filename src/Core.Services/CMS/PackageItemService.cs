using Core.Objects;
using Core.Objects.Entities.Packaging;

namespace Core.Services.CMS
{
    public class PackageItemService : CoreService<PackageItem>
    {
        public PackageItemService(ICoreDataContext db) : base(db) { }
    }
}