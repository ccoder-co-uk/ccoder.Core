using cCoder.Core.Objects;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.Packaging;

public class CorePackageInstaller : PackageInstaller
{
    public CorePackageInstaller(ILogger<CorePackageInstaller> log, ICoreDataContext db, IEnumerable<IPackageItemImporter> importers)
        : base(log, db, importers.Where(i => i.Type.StartsWith("Core")).OrderBy(i => i.Order).ToList()) { }
}