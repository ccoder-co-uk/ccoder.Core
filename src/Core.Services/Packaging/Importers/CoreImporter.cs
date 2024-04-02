using Core.Objects.Entities.Security;
using Core.Services;

namespace Core.Packaging.Importers
{
    public abstract class CoreImporter<T> : Importer<T, User> where T : class
    {
        protected CoreImporter(ICoreService<T> service, string type) : base(service, type) { }
    }
}