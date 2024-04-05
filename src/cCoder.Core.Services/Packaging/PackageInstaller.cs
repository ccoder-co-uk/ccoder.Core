using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Packaging
{
    public abstract class PackageInstaller : IPackageInstaller
    {
        public string Type { get; set; }
        protected ICoreDataContext Core { get; private set; }
        protected IEnumerable<IPackageItemImporter> Importers { get; set; }

        protected PackageInstaller(ICoreDataContext db, IEnumerable<IPackageItemImporter> importers)
        {
            Core = db;
            Importers = importers;
        }

        public virtual async Task Import(int appId, Package package)
        {
            if (Core.User.IsAdminOfApp(appId))
            {
                foreach (PackageItem item in package?.Items)
                {
                    IPackageItemImporter[] importers = Importers.Where(i => i.Type == item.Type).ToArray();
                    foreach (IPackageItemImporter importer in importers)
                        await importer.Import(appId, item);
                }
            }
            else
                throw new SecurityException("Access Denied!");
        }
    }
}
