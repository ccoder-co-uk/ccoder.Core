using cCoder.Core.Objects;

namespace cCoder.Core.Services.Packaging;

public class CorePackageInstaller : PackageInstaller
{
    public CorePackageInstaller(ICoreDataContext db, IEnumerable<IPackageItemImporter> importers)
        : base(db, importers.Where(i => i.Type.StartsWith("Core")).OrderBy(i => i.Order).ToList()) { }
}