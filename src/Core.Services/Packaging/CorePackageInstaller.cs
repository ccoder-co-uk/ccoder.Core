using cCoder.Core.Objects;
using System.Collections.Generic;
using System.Linq;

namespace cCoder.Core.Packaging
{
    public class CorePackageInstaller : PackageInstaller
    {
        public CorePackageInstaller(ICoreDataContext db, IEnumerable<IPackageItemImporter> importers)
            : base(db, importers.Where(i => i.Type.StartsWith("Core")).OrderBy(i => i.Order).ToList()) { }
    }
}