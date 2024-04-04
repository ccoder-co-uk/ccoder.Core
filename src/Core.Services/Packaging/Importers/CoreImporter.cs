using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;

namespace cCoder.Core.Packaging.Importers
{
    public abstract class CoreImporter<T> : Importer<T, User> where T : class
    {
        protected CoreImporter(ICoreService<T> service, string type) : base(service, type) { }
    }
}