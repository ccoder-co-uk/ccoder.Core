using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;

namespace cCoder.Core.Services.CMS;

public class PackageItemService : CoreService<PackageItem>
{
    public PackageItemService(ICoreDataContext db) : base(db) { }
}