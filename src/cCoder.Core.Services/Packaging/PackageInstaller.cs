using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;
using Microsoft.Extensions.Logging;
using System.Security;

namespace cCoder.Core.Services.Packaging;

public abstract class PackageInstaller : IPackageInstaller
{
    private readonly ILogger log;

    public string Type { get; set; }
    protected ICoreDataContext Core { get; private set; }
    protected IEnumerable<IPackageItemImporter> Importers { get; set; }

    protected PackageInstaller(ILogger log, ICoreDataContext db, IEnumerable<IPackageItemImporter> importers)
    {
        this.log = log;
        Core = db;
        Importers = importers;
    }

    public virtual async Task Import(int appId, Package package)
    {
        if (package.Items is null)
            return;

        if (Core.User.IsAdminOfApp(appId))
            foreach (PackageItem item in package.Items)
            {
                IPackageItemImporter[] importers = Importers.Where(i => i.Type == item.Type).ToArray();
                log.LogDebug("Importing {ItemType} from {PackageSource}", item.Type, package.SourceApi);

                foreach (IPackageItemImporter importer in importers)
                    await importer.Import(appId, item);
            }
        else
            throw new SecurityException("Access Denied!");
    }
}